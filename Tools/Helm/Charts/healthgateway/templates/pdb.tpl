{{- define "pdb.tpl" }}
{{- $top := index . 0 -}}
{{- $context := index . 1 -}}
{{- $name := printf "%s-%s" $top.Release.Name $context.name -}}
{{- $namespace := $top.Values.namespace | default $top.Release.Namespace -}}
{{- $labels := include "standard.labels" $top -}}
{{- $minAvailable := ($context.scaling).pdbMinAvailable | default ($top.Values.scaling).pdbMinAvailable | required "pdbMinAvailable required" -}}
{{- $replicas := (kindIs "float64" $context.replicas) | ternary $context.replicas (default ($top.Values.scaling).dcReplicas | required "dcReplicas required") -}}
{{- if and (ne 0.0 $minAvailable) (ne 0.0 $replicas) -}}
kind: PodDisruptionBudget
apiVersion: policy/v1
metadata:
  name: {{ $name }}-pdb
  namespace: {{ $namespace }}
  labels: {{ $labels | nindent 4 }}
spec:
  minAvailable: {{ $minAvailable }}
  selector:
    matchLabels:
      deploymentconfig: {{ $name }}-dc
{{- end -}}
{{- end -}}
