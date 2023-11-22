{{- $name := include "standard.name" . -}}
kind: PodDisruptionBudget
apiVersion: policy/v1
metadata:
  name: {{ $name }}-pdb
  labels: {{ include "standard.labels" . | nindent 4 }}
spec:
  minAvailable: 1
  selector:
    matchLabels:
      name: {{ $name }}
