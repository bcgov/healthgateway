# deployment template
{{- define "deployment.tpl" }}
{{- $top := index . 0 -}}
{{- $context := index . 1 -}}
{{- $name := printf "%s-%s" $top.Release.Name $context.name -}}
{{- $namespace := $top.Values.namespace | default $top.Release.Namespace -}}
{{- $labels := include "standard.labels" $top -}}
{{- $port := ($context.port | default 8080) -}}
{{- $protocol := ($context.protocol | default "tcp") -}}
{{- $image := print $top.Values.defaultImageRepository "/" $top.Values.toolsNamespace "/" ($context.image.imageStreamName | default $context.name) -}}
{{- $tag := $context.image.tag | default $top.Values.defaultImageTag | default "latest" -}}
{{- $replicas := (kindIs "float64" $context.replicas) | ternary $context.replicas (default ($top.Values.scaling).deploymentReplicas | required "deploymentReplicas required") -}}
{{- $role := $context.role -}}
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ $name }}-deployment
  namespace: {{ $namespace }}
  labels:
    {{- $labels | trim | nindent 4 }}
    role: {{ $role }}
  annotations:
    image.openshift.io/triggers: '[{"from":{"kind":"ImageStreamTag","name":"{{ $context.name }}:{{ $tag }}","namespace":"{{ $top.Values.toolsNamespace }}"},"fieldPath":"spec.template.spec.containers[?(@.name==\"{{ $name }}\")].image","pause":"false"}]'
spec:
  replicas: {{ $replicas }}
  revisionHistoryLimit: 10
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 50%
      maxSurge: 50%
  selector:
    matchLabels:
      name: {{ $name }}
  template:
    metadata:
      name: {{ $name }}
      labels:
        name: {{ $name }}
        role: {{ $role }}
      annotations:
        checksum/config: {{ $top.Values.commonConfig | toYaml | sha256sum }}
        checksum/secret: {{ $top.Values.commonSecrets.file | sha256sum }}-{{ $top.Files.Get $top.Values.commonSecrets.file | sha256sum }}
        checksum/files: {{ $top.Values.files | toYaml | sha256sum }}
        {{- range $file := $top.Values.files }}
        checksum/{{ $file.name }}: {{ $top.Files.Get $file.file | sha256sum }}
        {{- end }}
        {{- range $key, $value := $context.secrets }}
        {{ print "checksum/" $value.name  "-secrets"}}: {{ $top.Files.Get $value.file | toYaml | sha256sum }}
        {{- end }}
    spec:
      restartPolicy: Always
      containers:
        - name: {{ $name }}
          image: {{ $image }}:{{ $tag }}
          imagePullPolicy: Always
          resources:
            limits:
              cpu: {{ $context.limitCpu | default ($top.Values.defaultResources).limitCpu | default "150m" }}
              memory: {{ $context.limitMemory | default ($top.Values.defaultResources).limitMemory | default "512Mi" }}
            requests:
              cpu: {{ $context.requestCpu | default ($top.Values.defaultResources).requestCpu | default "50m" }}
              memory: {{ $context.requestMemory | default ($top.Values.defaultResources).requestMemory | default "256Mi" }}
          volumeMounts:
            - mountPath: /dpkeys
              name: dp
            - mountPath: /ssl
              name: ssl
              readOnly: true
          {{- range $fileKey, $fileValue := $context.files }}
            - name: {{ $fileValue.name }}
              mountPath: {{ printf "%s/%s" $fileValue.mountPath $fileValue.mountFileName | quote }}
              subPath: {{ $fileValue.mountFileName | quote }}
              readOnly: {{ $fileValue.readonly | default false }}
          {{- end }}
          envFrom:
            - configMapRef:
                name: {{ print $top.Release.Name "-common-config" }}
            - secretRef:
                name: {{ print $top.Release.Name "-common-secrets" }}
          {{- range $secretValue := $context.secrets }}
            - secretRef:
                name: {{ printf "%s-%s-secrets" $name $secretValue.name }}
          {{- end }}
          env: {{- include "all.dictionary" $context.config | nindent 12 }}
          ports:
            - containerPort: {{ $port }}
              protocol: {{ $protocol | upper }}
          livenessProbe:
            httpGet:
              path: {{ $context.livenessProbePath | default "/health" }}
              port: {{ $context.livenessProbePort | default $port }}
              scheme: HTTP
            timeoutSeconds: {{ $context.livenessProbeTimeout | default 5 }}
            periodSeconds: {{ $context.livenessProbePeriod | default 10 }}
            failureThreshold: {{ $context.livenessProbeFailureThreshold | default 5 }}
          readinessProbe:
            httpGet:
              path: {{ $context.readinessProbePath | default "/health" }}
              port: {{ $context.readinessProbePort | default $port }}
              scheme: HTTP
            timeoutSeconds: {{ $context.readinessProbeTimeout | default 10 }}
            periodSeconds: {{ $context.readinessProbePeriod | default 15 }}
            failureThreshold: {{ $context.readinessProbeFailureThreshold | default 5 }}
          startupProbe:
            httpGet:
              path: {{ $context.startupProbePath | default "/health" }}
              port: {{ $context.startupProbePort | default $port }}
              scheme: HTTP
            initialDelaySeconds: {{ $context.startupProbeInitialDelay | default 15 }}
            timeoutSeconds: {{ $context.startupProbeTimeout | default 5 }}
            periodSeconds: {{ $context.startupProbePeriod | default 10 }}
            failureThreshold: {{ $context.startupProbeFailureThreshold | default 5 }}
      volumes:
        - name: dp
          persistentVolumeClaim:
            claimName: {{ $top.Release.Name }}-data-protection
        - name: ssl
          secret:
            secretName: {{ $name }}-ssl
        {{- range $fileKey, $fileValue := $context.files }}
        - name: {{ $fileValue.name }}
          configMap:
            name: {{ printf "%s-files" $top.Release.Name }}
        {{- end }}
{{- end }}