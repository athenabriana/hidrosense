# language: pt
@api @health
Funcionalidade: Verificação de saúde da API
  Para que a orquestração só envie tráfego a uma instância que alcança o banco
  Como plataforma de deploy (Docker e Kubernetes)
  Quero um endpoint público de health check

  @positivo
  Cenário: API saudável com o banco de dados disponível
    Quando verifico a saúde da API
    Então o status da resposta deve ser 200
    E a resposta deve ser o texto "Healthy"
