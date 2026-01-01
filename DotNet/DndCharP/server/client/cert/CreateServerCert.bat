openssl genrsa -des3 -out server.key 2048
openssl req -new -key server.key -out server.csr -config configServer.txt
openssl x509 -req -in server.csr -CA root.crt -CAkey root.key -CAcreateserial -out server.crt -days 365 -sha256 -extfile configServer.txt -extensions req_ext

openssl pkcs12 -inkey server.key -in server.crt -export -out server.pfx
openssl pkcs12 -in server.pfx -nokeys -nodes -out server.pem
openssl x509 -inform PEM -in server.pem -outform DER -out server.cer

openssl rsa -in server.key -out serverKey.pem