package ca.bc.gov.healthgateway.legacycleanup

import android.content.Context
import android.content.pm.ApplicationInfo
import android.util.Log
import java.io.File
import java.security.KeyStore
import java.util.concurrent.Executors

private const val TAG = "HgLegacyCleanup"
private const val PREFS = "hg_legacy_cleanup"
private const val SWEPT = "done"

private const val LEGACY_DATABASE = "my_health_db"
private const val LEGACY_PREFS = "my_health_preferences"
/** androidx.security.crypto's default master key, which only the old app's preferences used. */
private const val LEGACY_KEY_ALIAS = "_androidx_security_master_key_"
private const val WORK_DATABASE = "androidx.work.workdb"
private const val WORK_PREFS = "androidx.work.util.preferences"
private const val SNOWPLOW = "snowplow"
/** The old app's `createTempFile("file", ".pdf", filesDir)`. This app's own files are inside hg-record-cache. */
private val TEMP_PDF = Regex("^file\\d+\\.pdf$")
private val DATABASE_SUFFIXES = listOf("", "-journal", "-shm", "-wal")
private const val PREFS_BACKUP_SUFFIX = ".bak"
/** A preferences file, and the backup Android keeps beside it while it writes. The backup's suffix is tried first. */
private val PREFS_SUFFIXES = listOf(".xml$PREFS_BACKUP_SUFFIX", ".xml")

/**
 * The store app has the ID of the old Health Gateway app (myhealthBC-android), so an update keeps everything the old app
 * saved: its copy of the user's records, its login and protective word, and its analytics queue (legacy.md). They are
 * removed by name at the first launch of an install, and at each later one until a run removes them all. Android needs no
 * fresh-install guard: uninstalling deletes the app's data and its Keystore keys, and the old app allowed no backups.
 */
internal object LegacyCleanup {
  // one thread: a report asked for during the sweep waits for it
  private val executor = Executors.newSingleThreadExecutor()
  /** What this launch's sweep did. Read and written on `executor` only. */
  private var launchReport: List<Group>? = null

  class Group(val name: String) {
    var found = 0
    var removed = 0
    var failed = 0

    fun count(deleted: Boolean) {
      found += 1
      if (deleted) removed += 1 else failed += 1
    }

    fun asMap(): Map<String, Any> = mapOf("group" to name, "found" to found, "removed" to removed, "failed" to failed)
  }

  /** `Application.onCreate`, before the JavaScript bundle loads. None of the old names are anything this app reads. */
  fun runAtLaunch(context: Context) {
    if (flags(context).getBoolean(SWEPT, false)) return
    executor.execute {
      val groups = sweep(context)
      launchReport = groups
      // names and counts only
      for (group in groups) {
        if (group.found > 0) {
          Log.i(TAG, "Legacy cleanup ${group.name}: found ${group.found}, removed ${group.removed}, failed ${group.failed}")
        }
      }
    }
  }

  /** The launch's report, once the sweep has finished. Null: nothing ran, as it was all done at an earlier launch. */
  fun report(callback: (List<Map<String, Any>>?) -> Unit) {
    executor.execute { callback(launchReport?.map { it.asMap() }) }
  }

  /** Reset App: the same deletes, whatever the flag says. */
  fun removeNow(context: Context, callback: (List<Map<String, Any>>) -> Unit) {
    executor.execute { callback(sweep(context).map { it.asMap() }) }
  }

  private fun flags(context: Context) = context.getSharedPreferences(PREFS, Context.MODE_PRIVATE)

  private fun sweep(context: Context): List<Group> {
    val groups = listOf(
      Group("database").also { deleteDatabase(context, context.getDatabasePath(LEGACY_DATABASE), it) },
      Group("preferences").also { deletePreferences(context, LEGACY_PREFS, it) },
      Group("key").also { deleteKey(it) },
      // WorkManager's records of the old app's one-off work: nothing in this app uses WorkManager
      Group("workManager").also {
        deleteDatabase(context, context.getDatabasePath(WORK_DATABASE), it)
        // where WorkManager 2.x keeps it
        deleteDatabase(context, File(context.noBackupFilesDir, WORK_DATABASE), it)
        deletePreferences(context, WORK_PREFS, it)
      },
      Group("analytics").also { deleteSnowplow(context, it) },
      Group("pdf").also { group ->
        list(context.filesDir, group).filter { it.isFile && TEMP_PDF.matches(it.name) }
          .forEach { delete(context, it, group) }
      },
    )
    if (groups.all { it.failed == 0 }) flags(context).edit().putBoolean(SWEPT, true).apply()
    return groups
  }

  private fun deleteDatabase(context: Context, path: File, group: Group) {
    for (suffix in DATABASE_SUFFIXES) {
      File(path.path + suffix).takeIf { it.exists() }?.let { delete(context, it, group) }
    }
  }

  private fun deletePreferences(context: Context, name: String, group: Group) {
    // A write that was cut short leaves only the backup, an older copy of the same data: deleteSharedPreferences removes
    // both files, and fails if either is still there.
    val file = preferencesFile(context, name)
    if (!file.exists() && !File(file.path + PREFS_BACKUP_SUFFIX).exists()) return
    val deleted = try {
      !forcedFailure(context) && context.deleteSharedPreferences(name)
    } catch (error: RuntimeException) {
      false
    }
    if (!deleted) Log.w(TAG, "Legacy cleanup: could not delete from ${group.name}")
    group.count(deleted)
  }

  private fun deleteKey(group: Group) {
    try {
      val keyStore = KeyStore.getInstance("AndroidKeyStore").apply { load(null) }
      if (!keyStore.containsAlias(LEGACY_KEY_ALIAS)) return
      keyStore.deleteEntry(LEGACY_KEY_ALIAS)
      group.count(true)
    } catch (error: Exception) {
      Log.w(TAG, "Legacy cleanup: could not delete from ${group.name}: ${error.javaClass.simpleName}")
      group.count(false)
    }
  }

  /** Snowplow Android 2.x's event queue and session. This app's tracker is JavaScript only and uses AsyncStorage. */
  private fun deleteSnowplow(context: Context, group: Group) {
    list(context.getDatabasePath(SNOWPLOW).parentFile, group)
      .filter { it.isFile && it.name.contains(SNOWPLOW, ignoreCase = true) }
      .forEach { delete(context, it, group) }
    list(preferencesFile(context, SNOWPLOW).parentFile, group)
      .filter { it.name.contains(SNOWPLOW, ignoreCase = true) }
      .mapNotNull { file ->
        PREFS_SUFFIXES.firstOrNull { file.name.endsWith(it) }?.let { file.name.removeSuffix(it) }
      }
      .distinct()
      .forEach { deletePreferences(context, it, group) }
  }

  /**
   * A folder's entries. A folder that exists but can't be read counts as a failure, so the flag stays unset and the next
   * launch looks again: `listFiles` returns null for that, the same as for a folder that was never made.
   */
  private fun list(folder: File?, group: Group): List<File> {
    if (folder == null || !folder.exists()) return emptyList()
    val entries = folder.listFiles()
    if (entries == null) {
      Log.w(TAG, "Legacy cleanup: could not list a folder for ${group.name}")
      group.count(false)
      return emptyList()
    }
    return entries.asList()
  }

  private fun delete(context: Context, file: File, group: Group) {
    val deleted = !forcedFailure(context) && file.delete()
    if (!deleted) Log.w(TAG, "Legacy cleanup: could not delete from ${group.name}")
    group.count(deleted)
  }

  private fun preferencesFile(context: Context, name: String) = File(context.dataDir, "shared_prefs/$name.xml")

  /**
   * Debuggable builds, for the test plan: `adb shell run-as <app ID> touch files/hg.legacyCleanup.forceFailure` fails
   * every file delete.
   */
  private fun forcedFailure(context: Context) =
    context.applicationInfo.flags and ApplicationInfo.FLAG_DEBUGGABLE != 0 &&
      File(context.filesDir, "hg.legacyCleanup.forceFailure").exists()
}
