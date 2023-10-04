# HPA policy template
{{- define "hpa.tpl" }}
{{- $top := index . 0 -}}
{{- $context := index . 1 -}}
{{- if or (or (not (hasKey $context "scaling")) (not (hasKey $context.scaling "enabled"))) ($context.scaling).enabled -}}
    {{- $name := printf "%s-%s" $top.Release.Name $context.name -}}
    {{- $namespace := $top.Values.namespace | default $top.Release.Namespace -}}
    {{- $labels := include "standard.labels" $top -}}
    {{- $minReplicas := ($context.scaling).hpaMinReplicas | default $top.Values.scaling.hpaMinReplicas | required "hpaMinReplicas required" -}}
    {{- $maxReplicas := ($context.scaling).hpaMaxReplicas | default $top.Values.scaling.hpaMaxReplicas | required "hpaMaxReplicas required" -}}
    {{- $cpuUtilization := ($context.scaling).hpaCpuUtilitzation | default $top.Values.scaling.hpaCpuUtilitzation | default 150 -}}
    {{- $memoryUtilization := ($context.scaling).hpaMemoryUtilitzation | default $top.Values.scaling.hpaMemoryUtilitzation | default 150 -}}
kind: HorizontalPodAutoscaler
apiVersion: autoscaling/v2
metadata:
  name: {{ $name }}-hpa
  namespace: {{ $namespace }}
  labels: {{ $labels | nindent 4 }}
spec:
  scaleTargetRef:
    kind: DeploymentConfig
    name: {{ $name }}-dc
    apiVersion: apps.openshift.io/v1
  minReplicas: {{ $minReplicas }}
  maxReplicas: {{ $maxReplicas }}
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: {{ $cpuUtilization }}
    - type: Resource
      resource:
        name: memory
        target:
          type: Utilization
          averageUtilization:  {{ $memoryUtilization }}
{{- end -}}
{{- end -}}
