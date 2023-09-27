# helper templates
/* standard labels */
{{- define "standard.labels" -}}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/instance: {{ .Release.Name }}
helm.sh/chart: {{ .Chart.Name }}-{{ .Chart.Version | replace "+" "_" }}
dr: {{ .Values.isDr | toString | quote }}
{{- end -}}

/* map all dictionary values - useful for configmap data */
{{- define "all.data" -}}
{{- range $key, $value := . }}
{{ $key }}: {{ $value | quote }}
{{- end -}}
{{- end -}}

/* map all dictionary values as base64 - useful for secret and binary data */
{{- define "all.encoded" -}}
{{- range $key, $value := . }}
{{ $key }}: {{ $value | toString | b64enc | quote }}
{{- end -}}
{{- end -}}

/* map all dictionary values as key/value pairs - useful for inline env values */
{{- define "all.dictionary" -}}
{{- range $key, $value := . }}
- name: {{ $key }}
  value: {{ $value | quote }}
{{- end -}}
{{- end -}}
