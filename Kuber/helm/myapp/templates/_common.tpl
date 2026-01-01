{{- define "all_templated_volumes" }}
- name: servercert
  secret:
    secretName: {{ .Chart.Name }}-servercert
    items:
    - key: tls.crt
      path: cert.crt
    - key: tls.key
      path: cert.key
{{- end }}

{{- define "all_templated_mounts" }}
- name: servercert
  mountPath: /app/servercert
  readOnly: true
{{- end }}
