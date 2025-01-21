# ingress template
{{- define "ingress.tpl" }}
{{- $top := index . 0 -}}
{{- $context := index . 1 -}}
{{- $name := printf "%s-%s" $top.Release.Name $context.name -}}
{{- $namespace := $top.Values.namespace | default $top.Release.Namespace -}}
{{- $labels := include "standard.labels" $top -}}
{{- $port := ($context.port | default 8080) -}}
{{- $protocol := ($context.protocol | default "tcp") -}}
{{- $timeout := $context.routeTimeout | default "60s" -}}
{{- range $host := $context.hosts }}
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: {{ $name }}-{{ $host.name | default $host.host }}-ing
  namespace: {{ $namespace }}
  labels: {{ $labels | nindent 4 }}
  annotations:
    nginx.ingress.kubernetes.io/hsts: "true"
    nginx.ingress.kubernetes.io/hsts-max-age: "31536000"
    nginx.ingress.kubernetes.io/hsts-include-subdomains: "true"
    nginx.ingress.kubernetes.io/hsts-preload: "true"
    nginx.ingress.kubernetes.io/proxy-connect-timeout: {{ $timeout }}
    nginx.ingress.kubernetes.io/proxy-read-timeout: {{ $timeout }}
    nginx.ingress.kubernetes.io/load-balance: "least_conn"
spec:
  tls:
  - hosts:
    - {{ $host.host }}
    secretName: {{ $name }}-tls-secret
  rules:
  - host: {{ $host.host }}
    http:
      paths:
      - path: {{ $host.path | default "/" }}
        pathType: Prefix
        backend:
          service:
            name: {{ $name }}-svc
            port:
              name: {{ printf "%s-%s" ($port | toString) $protocol }}
---
{{- end -}}
{{- end -}}
