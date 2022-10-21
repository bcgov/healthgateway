resource "keycloak_user" "user_nisamson" {
  realm_id = data.keycloak_realm.hg_realm.id
  username = "nisamsons@idir"
  enabled  = true
}
resource "keycloak_user_roles" "user_roles_nisamson" {
  realm_id = data.keycloak_realm.hg_realm.id
  user_id  = keycloak_user.user_nisamson.id
  role_ids = [
    data.keycloak_role.default_hg.id,
    data.keycloak_role.realm_admin.id,
    keycloak_role.AdminUser.id
  ]
}

resource "keycloak_user" "user_sslaws" {
  realm_id = data.keycloak_realm.hg_realm.id
  username = "sslaws@idir"
  enabled  = true
}
resource "keycloak_user_roles" "user_roles_sslaws" {
  realm_id = data.keycloak_realm.hg_realm.id
  user_id  = keycloak_user.user_sslaws.id
  role_ids = [
    data.keycloak_role.default_hg.id,
    data.keycloak_role.realm_admin.id,
    keycloak_role.AdminUser.id
  ]
}

resource "keycloak_user" "user_bhead" {
  realm_id = data.keycloak_realm.hg_realm.id
  username = "bhead@idir"
  enabled  = true
}
resource "keycloak_user_roles" "user_roles_bhead" {
  realm_id = data.keycloak_realm.hg_realm.id
  user_id  = keycloak_user.user_bhead.id
  role_ids = [
    data.keycloak_role.default_hg.id,
    data.keycloak_role.realm_viewer.id,
    keycloak_role.AdminUser.id
  ]
}

resource "keycloak_user" "user_brjang" {
  realm_id = data.keycloak_realm.hg_realm.id
  username = "brjang@idir"
  enabled  = true
}
resource "keycloak_user_roles" "user_roles_brjang" {
  realm_id = data.keycloak_realm.hg_realm.id
  user_id  = keycloak_user.user_brjang.id
  role_ids = [
    data.keycloak_role.default_hg.id,
    data.keycloak_role.realm_viewer.id,
    keycloak_role.AdminUser.id
  ]
}

resource "keycloak_user" "user_rayking" {
  realm_id = data.keycloak_realm.hg_realm.id
  username = "rayking@idir"
  enabled  = true
}
resource "keycloak_user_roles" "user_roles_rayking" {
  realm_id = data.keycloak_realm.hg_realm.id
  user_id  = keycloak_user.user_rayking.id
  role_ids = [
    data.keycloak_role.default_hg.id,
    data.keycloak_role.realm_viewer.id,
    keycloak_role.AdminUser.id
  ]
}

resource "keycloak_user" "user_miklyttl" {
  realm_id = data.keycloak_realm.hg_realm.id
  username = "miklyttl@idir"
  enabled  = true
}
resource "keycloak_user_roles" "user_roles_miklyttl" {
  realm_id = data.keycloak_realm.hg_realm.id
  user_id  = keycloak_user.user_miklyttl.id
  role_ids = [
    data.keycloak_role.default_hg.id,
    data.keycloak_role.realm_viewer.id,
    keycloak_role.AdminUser.id
  ]
}

resource "keycloak_user" "user_hg" {
  count      = local.development ? 1 : 0
  realm_id   = data.keycloak_realm.hg_realm.id
  username   = "healthgateway"
  enabled    = true
  first_name = "Dr"
  last_name  = "Gateway"
  attributes = {
    hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A"
  }
  initial_password {
    value     = var.user_default_password
    temporary = false
  }
}

resource "keycloak_user" "user_02" {
  count      = local.development ? 1 : 0
  realm_id   = data.keycloak_realm.hg_realm.id
  username   = "hthgtwy02"
  enabled    = true
  first_name = "Dr"
  last_name  = "Gateway"
  attributes = {
    hdid = "S22BPV6WHS5TRLBL4XKGQDBVDUKLPIRSBGYSEJAHYMYRP22SP2TA"
  }
  initial_password {
    value     = var.user_default_password
    temporary = false
  }
}

resource "keycloak_user" "user_03" {
  count      = local.development ? 1 : 0
  realm_id   = data.keycloak_realm.hg_realm.id
  username   = "hthgtwy03"
  enabled    = true
  first_name = "Dr"
  last_name  = "Gateway"
  attributes = {
    hdid = "R43YCT4ZY37EIJLW2O5LV2I77BZA3K3M25EUJGWAVGVJ7JKBDKCQ"
  }
  initial_password {
    value     = var.user_default_password
    temporary = false
  }
}

resource "keycloak_user" "user_04" {
  count      = local.development ? 1 : 0
  realm_id   = data.keycloak_realm.hg_realm.id
  username   = "hthgtwy04"
  enabled    = true
  first_name = "Dr"
  last_name  = "Gateway"
  attributes = {
    hdid = "K6HL4VX67CZ2PGSZ2ZOIR4C3PGMFFBW5CIOXM74D6EQ7RYYL7P4A"
  }
  initial_password {
    value     = var.user_default_password
    temporary = false
  }
}

resource "keycloak_user" "user_09" {
  count      = local.development ? 1 : 0
  realm_id   = data.keycloak_realm.hg_realm.id
  username   = "hthgtwy09"
  enabled    = true
  first_name = "Dr"
  last_name  = "Gateway"
  attributes = {
    hdid = "3ZQCSNNC6KVP2GYLA4O3EFZXGUAPWBQHU6ZEB7FXNZJ2WYCLPH3A"
  }
  initial_password {
    value     = var.user_default_password
    temporary = false
  }
}

resource "keycloak_user" "user_12" {
  count      = local.development ? 1 : 0
  realm_id   = data.keycloak_realm.hg_realm.id
  username   = "healthgateway12"
  enabled    = true
  first_name = "Dr"
  last_name  = "Gateway"
  attributes = {
    hdid = "L6XJCCRMCCZQ2L6LCVRLBWGCRGWMBPI6OZVXDG5AYPSJ2OPYB2WQ"
  }
  initial_password {
    value     = var.user_default_password
    temporary = false
  }
}

resource "keycloak_user" "user_19" {
  count      = local.development ? 1 : 0
  realm_id   = data.keycloak_realm.hg_realm.id
  username   = "hthgtwy19"
  enabled    = true
  first_name = "Deceased"
  last_name  = "Gateway"
  attributes = {
    hdid = "MPOLGP66AV4PPDB6ZMYEWQ63WKRYPM4EPDW5MSXA2LA65EQOEMCQ"
  }
  initial_password {
    value     = var.user_default_password
    temporary = false
  }
}

resource "keycloak_user" "user_20" {
  count      = local.development ? 1 : 0
  realm_id   = data.keycloak_realm.hg_realm.id
  username   = "hthgtwy20"
  enabled    = true
  first_name = "Dr"
  last_name  = "Gateway"
  attributes = {
    hdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA"
  }
  initial_password {
    value     = var.user_default_password
    temporary = false
  }
}

resource "keycloak_user" "user_401" {
  count      = local.development ? 1 : 0
  realm_id   = data.keycloak_realm.hg_realm.id
  username   = "hlthgw401"
  enabled    = true
  first_name = "Dr"
  last_name  = "Gateway"
  attributes = {
    hdid = "C54JQKXANHJK7TIYBRCHJBOKXOIXASNQE76WSRCTOPKOXRMI5OAA"
  }
  initial_password {
    value     = var.user_default_password
    temporary = false
  }
}

resource "keycloak_user" "user_protected" {
  count      = local.development ? 1 : 0
  realm_id   = data.keycloak_realm.hg_realm.id
  username   = "protected"
  enabled    = true
  first_name = "Dr"
  last_name  = "Gateway"
  attributes = {
    hdid = "RD33Y2LJEUZCY2TCMOIECUTKS3E62MEQ62CSUL6Q553IHHBI3AWQ"
  }
  initial_password {
    value     = var.user_default_password
    temporary = false
  }
}

resource "keycloak_user" "user_closed" {
  count      = local.development ? 1 : 0
  realm_id   = data.keycloak_realm.hg_realm.id
  username   = "accountclosure"
  enabled    = true
  first_name = "Account"
  last_name  = "Closure"
  attributes = {
    hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ"
  }
  initial_password {
    value     = var.user_default_password
    temporary = false
  }
}



