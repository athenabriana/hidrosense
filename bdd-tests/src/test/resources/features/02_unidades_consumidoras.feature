# language: pt
@api @consumer-units
Funcionalidade: Cadastro de unidades consumidoras
  Para acompanhar o consumo de água de cada imóvel atendido
  Como administrador da companhia de saneamento
  Quero cadastrar e consultar as unidades consumidoras monitoradas

  @positivo
  Cenário: Listar unidades consumidoras com paginação
    Dado que estou autenticado como "operator"
    Quando consulto "/api/v1/consumer-units?page=1&pageSize=5"
    Então o status da resposta deve ser 200
    E a resposta deve obedecer ao contrato "consumer-unit-page.schema.json"
    E a lista "items" não deve estar vazia
    E o campo "page" deve valer 1
    E o campo "pageSize" deve valer 5

  @positivo
  Cenário: Filtrar unidades consumidoras por região
    Dado que estou autenticado como "operator"
    Quando consulto "/api/v1/consumer-units?region=Centro"
    Então o status da resposta deve ser 200
    E a resposta deve obedecer ao contrato "consumer-unit-page.schema.json"
    E todos os itens de "items" devem ter "region" igual a "Centro"

  @positivo
  Cenário: Administrador cadastra uma nova unidade consumidora
    Dado que estou autenticado como "admin"
    Quando cadastro uma unidade consumidora na região "Zona Sul" com vazão ociosa de 1.5 litros por minuto
    Então o status da resposta deve ser 201
    E a resposta deve obedecer ao contrato "consumer-unit.schema.json"
    E a unidade retornada deve ter o código enviado
    E o campo "region" deve ser "Zona Sul"
    E o campo "type" deve ser "Residential"
    E o campo "active" deve ser verdadeiro

  @negativo
  Cenário: Código de unidade duplicado é recusado
    Dado que estou autenticado como "admin"
    E que existe uma unidade consumidora com vazão ociosa de 1.0 litros por minuto
    Quando cadastro outra unidade consumidora com o mesmo código
    Então o status da resposta deve ser 409
    E a resposta deve obedecer ao contrato "problem.schema.json"
    E o campo "detail" deve conter "already exists"

  @negativo @governanca
  Cenário: Operador não tem permissão para cadastrar unidades
    Dado que estou autenticado como "operator"
    Quando cadastro uma unidade consumidora na região "Zona Sul" com vazão ociosa de 1.5 litros por minuto
    Então o status da resposta deve ser 403

  @negativo @governanca
  Cenário: Consulta sem token é bloqueada
    Dado que não estou autenticado
    Quando consulto "/api/v1/consumer-units"
    Então o status da resposta deve ser 401

  @negativo
  Esquema do Cenário: Dados inválidos no cadastro são rejeitados
    Dado que estou autenticado como "admin"
    Quando cadastro uma unidade consumidora com o campo "<campo>" valendo <valor>
    Então o status da resposta deve ser 400
    E a resposta deve obedecer ao contrato "validation-problem.schema.json"
    E o erro de validação deve apontar o campo "<campo>"

    Exemplos:
      | campo                | valor |
      | code                 | ""    |
      | region               | ""    |
      | monthlyBudgetLiters  | 0     |
      | idleFlowThresholdLpm | -1    |
      | type                 | 9     |
