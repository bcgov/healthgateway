/* standard labels */
{{- define "standard.labels" -}}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/instance: {{ .Release.Name }}
helm.sh/chart: {{ .Chart.Name }}-{{ .Chart.Version | replace "+" "_" }}
{{- end -}}

/* standard name */
{{- define "standard.name" -}}
{{ print .Release.Name "-" .Chart.Name }}
{{- end -}}
