# Script para visualizar mensagens na fila catalog.events.test

$rabbitMqApiUrl = "http://localhost:15672/api"
$rabbitUser = "guest"
$rabbitPass = "guest"
$base64Auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${rabbitUser}:${rabbitPass}"))
$headers = @{Authorization = "Basic $base64Auth"}

$queueName = "catalog.events.test"

Write-Host "=== Visualizando mensagens do CatalogAPI ===" -ForegroundColor Green

# Obter mensagens da fila (sem remover)
$getMessagesPayload = @{
	count = 10
	ackmode = "ack_requeue_true"
	encoding = "auto"
} | ConvertTo-Json

try {
	$messages = Invoke-RestMethod -Uri "$rabbitMqApiUrl/queues/%2f/$queueName/get" -Method Post -Headers $headers -Body $getMessagesPayload -ContentType "application/json"

	if ($messages.Count -eq 0) {
		Write-Host "`nNenhuma mensagem na fila '$queueName'" -ForegroundColor Yellow
		Write-Host "Execute o test-order-consumer.ps1 primeiro para gerar eventos" -ForegroundColor Yellow
	} else {
		Write-Host "`nEncontradas $($messages.Count) mensagem(ns) na fila '$queueName':" -ForegroundColor Cyan

		$index = 1
		foreach ($msg in $messages) {
			Write-Host "`n--- Mensagem $index ---" -ForegroundColor Magenta
			$payload = $msg.payload
			$eventData = $payload | ConvertFrom-Json

			Write-Host "Tipo: StockReservedEvent ou StockInsufficientEvent" -ForegroundColor White
			Write-Host "Payload:" -ForegroundColor White
			Write-Host ($payload | ConvertFrom-Json | ConvertTo-Json -Depth 5) -ForegroundColor Gray

			$index++
		}
	}
} catch {
	Write-Host "Erro ao obter mensagens: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Fim ===" -ForegroundColor Green
