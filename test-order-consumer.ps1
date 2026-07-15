# Script para testar o OrderEventConsumer do CatalogAPI
# Publica um evento OrderCreatedEvent no RabbitMQ e verifica se o estoque é reservado

$baseUrl = "http://localhost:5228/api/games"
$rabbitMqApiUrl = "http://localhost:15672/api"
$rabbitUser = "guest"
$rabbitPass = "guest"
$base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${rabbitUser}:${rabbitPass}"))
$headers = @{Authorization = "Basic $base64Auth"}

Write-Host "=== Teste OrderEventConsumer do CatalogAPI ===" -ForegroundColor Green

# 1. Criar um jogo para testar
Write-Host "`n1. Criando jogo de teste..." -ForegroundColor Cyan
$gameData = @{
	title = "Elden Ring"
	description = "RPG de ação e fantasia sombria"
	price = 249.90
	stock = 50
} | ConvertTo-Json

$game = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $gameData -ContentType "application/json"
Write-Host "Jogo criado: $($game.title) - ID: $($game.id) - Estoque inicial: $($game.stock)" -ForegroundColor Yellow
$gameId = $game.id

# 2. Verificar estoque atual
Write-Host "`n2. Estoque atual..." -ForegroundColor Cyan
$currentGame = Invoke-RestMethod -Uri "$baseUrl/$gameId" -Method Get
Write-Host "Estoque: $($currentGame.stock)" -ForegroundColor Yellow

# 3. Publicar evento OrderCreatedEvent no RabbitMQ
Write-Host "`n3. Publicando evento OrderCreatedEvent no RabbitMQ..." -ForegroundColor Cyan
$orderId = [Guid]::NewGuid().ToString()
$userId = [Guid]::NewGuid().ToString()

$orderEvent = @{
	OrderId = $orderId
	UserId = $userId
	GameId = $gameId
	Quantity = 3
	CreatedAt = (Get-Date).ToUniversalTime().ToString("o")
} | ConvertTo-Json

# Publicar no exchange order.exchange
$publishUrl = "$rabbitMqApiUrl/exchanges/%2f/order.exchange/publish"
$publishPayload = @{
	properties = @{}
	routing_key = ""
	payload = $orderEvent
	payload_encoding = "string"
} | ConvertTo-Json -Depth 5

try {
	Invoke-RestMethod -Uri $publishUrl -Method Post -Headers $headers -Body $publishPayload -ContentType "application/json" | Out-Null
	Write-Host "Evento publicado com sucesso!" -ForegroundColor Green
	Write-Host "OrderId: $orderId" -ForegroundColor White
	Write-Host "GameId: $gameId" -ForegroundColor White
	Write-Host "Quantity: 3" -ForegroundColor White
} catch {
	Write-Host "Erro ao publicar evento: $($_.Exception.Message)" -ForegroundColor Red
	exit 1
}

# 4. Aguardar processamento
Write-Host "`n4. Aguardando processamento pelo OrderEventConsumer..." -ForegroundColor Cyan
Start-Sleep -Seconds 3

# 5. Verificar se o estoque foi reservado
Write-Host "`n5. Verificando estoque após reserva..." -ForegroundColor Cyan
$updatedGame = Invoke-RestMethod -Uri "$baseUrl/$gameId" -Method Get
Write-Host "Estoque anterior: $($currentGame.stock)" -ForegroundColor White
Write-Host "Estoque atual: $($updatedGame.stock)" -ForegroundColor White
Write-Host "Diferença esperada: -3" -ForegroundColor White
Write-Host "Diferença real: $($updatedGame.stock - $currentGame.stock)" -ForegroundColor $(if ($updatedGame.stock -eq ($currentGame.stock - 3)) { "Green" } else { "Red" })

if ($updatedGame.stock -eq ($currentGame.stock - 3)) {
	Write-Host "`n✅ OrderEventConsumer funcionou corretamente!" -ForegroundColor Green
	Write-Host "Estoque foi reservado com sucesso." -ForegroundColor Green
} else {
	Write-Host "`n❌ OrderEventConsumer NÃO funcionou!" -ForegroundColor Red
	Write-Host "Estoque não foi atualizado como esperado." -ForegroundColor Red
}

# 6. Verificar evento publicado pelo CatalogAPI (StockReservedEvent)
Write-Host "`n6. Verificando eventos publicados pelo CatalogAPI..." -ForegroundColor Cyan
Write-Host "Para verificar o evento StockReservedEvent, crie uma fila e faça binding ao exchange catalog.exchange" -ForegroundColor Yellow

Write-Host "`n=== Teste concluído ===" -ForegroundColor Green
