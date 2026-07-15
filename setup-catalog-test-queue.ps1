# Script para criar fila de teste e fazer binding ao exchange catalog.exchange
# Para visualizar eventos StockReservedEvent publicados pelo CatalogAPI

$rabbitMqApiUrl = "http://localhost:15672/api"
$rabbitUser = "guest"
$rabbitPass = "guest"
$base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${rabbitUser}:${rabbitPass}"))
$headers = @{Authorization = "Basic $base64Auth"}

$queueName = "catalog.events.test"
$exchangeName = "catalog.exchange"

Write-Host "=== Configurando fila de teste para CatalogAPI ===" -ForegroundColor Green

# 1. Criar fila
Write-Host "`n1. Criando fila '$queueName'..." -ForegroundColor Cyan
$queuePayload = @{
	auto_delete = $false
	durable = $true
	arguments = @{}
} | ConvertTo-Json

try {
	Invoke-RestMethod -Uri "$rabbitMqApiUrl/queues/%2f/$queueName" -Method Put -Headers $headers -Body $queuePayload -ContentType "application/json" | Out-Null
	Write-Host "Fila criada com sucesso!" -ForegroundColor Green
} catch {
	Write-Host "Erro ao criar fila (talvez já exista): $($_.Exception.Message)" -ForegroundColor Yellow
}

# 2. Fazer binding da fila ao exchange
Write-Host "`n2. Fazendo binding da fila ao exchange '$exchangeName'..." -ForegroundColor Cyan
$bindingPayload = @{
	routing_key = ""
	arguments = @{}
} | ConvertTo-Json

try {
	Invoke-RestMethod -Uri "$rabbitMqApiUrl/bindings/%2f/e/$exchangeName/q/$queueName" -Method Post -Headers $headers -Body $bindingPayload -ContentType "application/json" | Out-Null
	Write-Host "Binding criado com sucesso!" -ForegroundColor Green
} catch {
	Write-Host "Erro ao criar binding (talvez já exista): $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`n✅ Configuração concluída!" -ForegroundColor Green
Write-Host "Agora execute o test-order-consumer.ps1 novamente e depois execute view-catalog-messages.ps1" -ForegroundColor Yellow
