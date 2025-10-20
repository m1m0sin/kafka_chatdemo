#!/usr/bin/env bash

set -e  # Detiene la ejecución si un comando falla

dispose_containers() {
    podman-compose down --volumes --remove-orphans
}

setup_containers() {
    podman-compose up -d
}

install_jdbc_connector() {
    echo "Instalando el conector JDBC..."
    ./init-jdbc-connector.sh
}

init_db() {
    echo "Inicializando la base de datos"
    ./init-db.sh
}

config_jdbc_connector() {
    echo "Configurando el conector JDBC para consumir los mensajes de kafka y almancenarlos en la base de datos..."
    ./jdbc-sink-chat-messages.sh
}

query_db() {
    echo "Querying data..."
    ./query-db.sh
}

main() {
    dispose_containers
    sleep 5

    setup_containers
    sleep 5

    install_jdbc_connector
    sleep 5

    init_db
    sleep 5

    config_jdbc_connector
    sleep 5

    query_db
}

main
