# CatalogAPI

API para gerenciar o catalogo de jogos da plataforma.

## O que faz

- Cadastrar novos jogos
- Buscar jogos disponiveis
- Atualizar informacoes e precos
- Controlar estoque dos jogos
- Reservar estoque quando alguem faz um pedido

## Como funciona

A API guarda os jogos no banco PostgreSQL. Quando um pedido chega pelo RabbitMQ, ela verifica se tem estoque e reserva as unidades. Se tiver estoque, avisa o PaymentsAPI que pode continuar com o pagamento.

## Como rodar

1. Ter Docker rodando com PostgreSQL e RabbitMQ
2. Entrar na pasta do projeto:
   ```
   cd CatalogAPI/src/CatalogAPI.Api
   ```
3. Rodar o comando:
   ```
   dotnet run
   ```
4. A API vai abrir em: http://localhost:5228

## O que precisa configurar

No arquivo appsettings.json tem:
- Conexao com banco PostgreSQL (CatalogDb)
- Configuracao do RabbitMQ
- Chave secreta do JWT

## Endpoints principais

- POST /api/games - Cadastrar jogo novo
- GET /api/games - Listar todos os jogos
- GET /api/games/{id} - Buscar um jogo especifico
- PUT /api/games/{id} - Atualizar jogo
- PATCH /api/games/{id}/stock - Ajustar estoque
- GET /api/health - Ver se a API esta funcionando

## Eventos que escuta

- OrderCreatedEvent - Quando um pedido e criado, reserva o estoque

## Eventos que publica

- StockReservedEvent - Avisa que o estoque foi reservado
- StockReservationFailedEvent - Avisa que nao tinha estoque
