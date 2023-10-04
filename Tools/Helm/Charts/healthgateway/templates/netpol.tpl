# network policy template
{{- define "netpol.tpl" }}
{{- $top := index . 0 -}}
{{- $context := index . 1 -}}
{{- $name := printf "%s-%s" $top.Release.Name $context.name -}}
{{- $namespace := $top.Values.namespace | default $top.Release.Namespace -}}
{{- $labels := include "standard.labels" $top -}}
{{- $port := ($context.port | default 8080) -}}
{{- $protocol := ($context.protocol | default "tcp") -}}
kind: NetworkPolicy
apiVersion: networking.k8s.io/v1
metadata:
  name: {{ $name }}-netpol
  namespace: {{ $namespace }}
  labels: {{ $labels | nindent 4 }}
spec:
  podSelector:
    matchLabels:
      name: {{ $name }}
  ingress:
    - from:
      - namespaceSelector:
          matchLabels:
            network.openshift.io/policy-group: ingress
      - podSelector:
          matchLabels:
            app.kubernetes.io/instance: {{ $top.Release.Name }}
      ports:
        - protocol: {{ $protocol | upper }}
          port: {{ $port }}
{{- end -}}
