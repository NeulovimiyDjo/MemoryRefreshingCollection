openssl genrsa -des3 -out client.key 2048
openssl req -new -key client.key -out client.csr -config configClient.txt
openssl x509 -req -in client.csr -CA rootCA.crt -CAkey rootCA.key -CAcreateserial -out client.crt -days 365 -sha256 -extfile configClient.txt -extensions req_ext

openssl pkcs12 -inkey client.key -in client.crt -export -out client.pfx
openssl pkcs12 -in client.pfx -nokeys -nodes -out clientCert.pem
openssl x509 -inform PEM -in clientCert.pem -outform DER -out client.cer