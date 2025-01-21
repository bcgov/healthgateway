# service template
{{- define "service.tpl" -}}
{{- $top := index . 0 -}}
{{- $context := index . 1 -}}
{{- $name := printf "%s-%s" $top.Release.Name $context.name -}}
{{- $namespace := $top.Values.namespace | default $top.Release.Namespace -}}
{{- $labels := include "standard.labels" $top -}}
{{- $port := $context.port | default 8080 -}}
{{- $protocol := $context.protocol | default "tcp" -}}
{{- $targetPort := $context.targetPort | default 8080 -}}

kind: Service
apiVersion: v1
metadata:
  name: {{ $name }}-svc
  namespace: {{ $namespace }}
  labels: {{ $labels | nindent 4 }}
  annotations:
    service.alpha.openshift.io/serving-cert-secret-name: {{ $name }}-ssl
spec:
  selector:
    app: {{ $name }}
    name: {{ $name }}
    role: {{ $context.role }}
  ports:
    - name: {{ (printf "%s-%s" ($port | toString) $protocol) }}
      port: {{ $port }}
      protocol: {{ $protocol | upper }}
      targetPort: {{ $targetPort }}
  type: ClusterIP
{{- end -}}
