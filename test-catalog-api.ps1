# Script de teste para CatalogAPI
$baseUrl = "http://localhost:5228/api/games"
$token = "" # Token JWT do UsersAPI (se necessário)

Write-Host "=== Teste CatalogAPI ===" -ForegroundColor Green

# 1. Criar jogo 1
Write-Host "`n1. Criando jogo 1..." -ForegroundColor Cyan
$game1 = @{
	title = "The Witcher 3"
	description = "RPG de mundo aberto aclamado pela crítica"
	price = 59.99
	stock = 100
} | ConvertTo-Json

$response1 = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $game1 -ContentType "application/json"
Write-Host "Jogo criado: $($response1.title) - ID: $($response1.id)" -ForegroundColor Yellow
$game1Id = $response1.id

# 2. Criar jogo 2
Write-Host "`n2. Criando jogo 2..." -ForegroundColor Cyan
$game2 = @{
	title = "Cyberpunk 2077"
	description = "RPG futurista em Night City"
	price = 49.99
	stock = 50
} | ConvertTo-Json

$response2 = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $game2 -ContentType "application/json"
Write-Host "Jogo criado: $($response2.title) - ID: $($response2.id)" -ForegroundColor Yellow
$game2Id = $response2.id

# 3. Listar todos os jogos
Write-Host "`n3. Listando todos os jogos (paginação)..." -ForegroundColor Cyan
$gamesUrl = "$baseUrl" + "?page=1&pageSize=10"  
$games = Invoke-RestMethod -Uri $gamesUrl -Method Get
Write-Host "Total de jogos: $($games.totalCount)" -ForegroundColor Yellow
Write-Host "Jogos na página:" -ForegroundColor Yellow
foreach ($game in $games.games) {
	Write-Host "  - $($game.title): R$ $($game.price) (Estoque: $($game.stock))" -ForegroundColor White
}

# 4. Buscar jogo por ID
Write-Host "`n4. Buscando jogo por ID ($game1Id)..." -ForegroundColor Cyan
$gameDetail = Invoke-RestMethod -Uri "$baseUrl/$game1Id" -Method Get
Write-Host "Encontrado: $($gameDetail.title)" -ForegroundColor Yellow
Write-Host "  Descrição: $($gameDetail.description)" -ForegroundColor White
Write-Host "  Preço: R$ $($gameDetail.price)" -ForegroundColor White
Write-Host "  Estoque: $($gameDetail.stock)" -ForegroundColor White

# 5. Atualizar informações do jogo
Write-Host "`n5. Atualizando informações do jogo..." -ForegroundColor Cyan
$updateData = @{
	id = $game1Id
	title = "The Witcher 3: Wild Hunt GOTY"
	description = "Edição Game of the Year com todas as expansões"
	price = 69.99
} | ConvertTo-Json

$updated = Invoke-RestMethod -Uri "$baseUrl/$game1Id" -Method Put -Body $updateData -ContentType "application/json"
Write-Host "Jogo atualizado: $($updated.title) - Novo preço: R$ $($updated.price)" -ForegroundColor Yellow

# 6. Adicionar estoque (quantidade positiva)
Write-Host "`n6. Adicionando 20 unidades ao estoque..." -ForegroundColor Cyan
$addStock = @{
	gameId = $game1Id
	quantity = 20
} | ConvertTo-Json

Invoke-RestMethod -Uri "$baseUrl/$game1Id/stock" -Method Patch -Body $addStock -ContentType "application/json"
$updated = Invoke-RestMethod -Uri "$baseUrl/$game1Id" -Method Get
Write-Host "Estoque atual: $($updated.stock)" -ForegroundColor Yellow

# 7. Reservar estoque (quantidade negativa)
Write-Host "`n7. Reservando 5 unidades do estoque..." -ForegroundColor Cyan
$reserveStock = @{
	gameId = $game1Id
	quantity = -5
} | ConvertTo-Json

Invoke-RestMethod -Uri "$baseUrl/$game1Id/stock" -Method Patch -Body $reserveStock -ContentType "application/json"
$updated = Invoke-RestMethod -Uri "$baseUrl/$game1Id" -Method Get
Write-Host "Estoque após reserva: $($updated.stock)" -ForegroundColor Yellow

# 8. Tentar reservar mais do que há em estoque (deve falhar)
Write-Host "`n8. Tentando reservar mais do que há em estoque (deve falhar)..." -ForegroundColor Cyan
$invalidReserve = @{
	gameId = $game1Id
	quantity = -999
} | ConvertTo-Json

try {
	Invoke-RestMethod -Uri "$baseUrl/$game1Id/stock" -Method Patch -Body $invalidReserve -ContentType "application/json"
	Write-Host "ERRO: Deveria ter falhado!" -ForegroundColor Red
} catch {
	Write-Host "Falhou corretamente: $($_.Exception.Message)" -ForegroundColor Green
}

Write-Host "`n=== Testes concluídos ===" -ForegroundColor Green
