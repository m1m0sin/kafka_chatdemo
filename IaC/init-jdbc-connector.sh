#!/usr/bin/env bash

check_status() {

  CONTAINER="connect"

  STATUS=$(podman inspect --format '{{.State.Status}}' "$CONTAINER")

  if [ "$STATUS" != "running" ]; then
    echo "🚀 El contenedor '$CONTAINER' no está corriendo (estado actual: $STATUS). Iniciando..."
    podman start "$CONTAINER"
  else
    echo "✅ El contenedor '$CONTAINER' ya está en ejecución."
  fi

  HEALTHSTATUS=$(podman inspect --format='{{.State.Health.Status}}' "$CONTAINER")

  until [ "$HEALTHSTATUS" = "healthy" ]; do
    echo "⏳ Esperando que '$CONTAINER' esté listo..."
    sleep 5
    HEALTHSTATUS=$(podman inspect --format='{{.State.Health.Status}}' "$CONTAINER")
  done
  echo "✅ '$CONTAINER' está arriba."

}

restart_connect() {
  echo "Reiniciando contenedor connect.."
  podman restart connect
}

check_status

podman exec -it connect bash -lc 'confluent-hub install --no-prompt confluentinc/kafka-connect-jdbc:latest && \
  (grep -q "^plugin.path=" /etc/kafka/connect-distributed.properties || \
   echo "plugin.path=/usr/share/java,/usr/share/confluent-hub-components" >> /etc/kafka/connect-distributed.properties)'

restart_connect
check_status

curl -s http://localhost:8083/connector-plugins | grep -i jdbc

podman exec -it connect bash -lc 'cd /usr/share/confluent-hub-components/confluentinc-kafka-connect-jdbc/lib && \
  (ls -1 | grep -i postgresql- || curl -LO https://jdbc.postgresql.org/download/postgresql-42.7.4.jar)'

restart_connect
check_status