# dc related secrets template
{{- define "secret.tpl" }}
{{- $top := index . 0 -}}
{{- $context := index . 1 -}}
{{- $name := printf "%s-%s" $top.Release.Name $context.name -}}
{{- $namespace := $top.Values.namespace | default $top.Release.Namespace -}}
{{- $labels := include "standard.labels" $top -}}
kind: Secret
apiVersion: v1
metadata:
  name: {{ $name }}-secrets
  namespace: {{ $namespace }}
  labels: {{ $labels | nindent 4 }}
data: {{ include "all.encoded" $context.values | nindent 2 }}
{{- end }}
