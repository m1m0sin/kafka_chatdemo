#!/usr/bin/env bash

# Crear topic ordersschema

podman exec -it kafka1 bash -lc \
  "kafka-topics --create --if-not-exists --topic ordersschema \
   --partitions 3 --replication-factor 3 --bootstrap-server kafka1:29092"

# Enviar mensajes al topic ordersschema sin modo interactivo

cat ordersschema.jsonl | podman exec -i kafka1 bash -lc \
  "kafka-console-producer --bootstrap-server kafka1:29092 --topic ordersschema"


