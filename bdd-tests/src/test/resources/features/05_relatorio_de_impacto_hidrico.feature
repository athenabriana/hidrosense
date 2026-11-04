# language: pt
@api @reports @esg @ambiental
Funcionalidade: Relatório de impacto hídrico
  Para priorizar as ações que mais reduzem o desperdício de água
  Como gestor de sustentabilidade
  Quero um relatório por região com consumo, vazamentos abertos e plano de mitigação

  @positivo
  Cenário: Operador gera o relatório de impacto hídrico
    Dado que estou autenticado como "operator"
    Quando consulto o relatório de impacto hídrico
    Então o status da resposta deve ser 200
    E a resposta deve obedecer ao contrato "water-impact-report.schema.json"
    E a lista "regions" não deve estar vazia

  @positivo
  Cenário: Vazamento aberto entra no relatório da região com plano de mitigação
    Dado que existe um alerta de vazamento aberto
    E que estou autenticado como "admin"
    Quando consulto o relatório de impacto hídrico
    Então o status da resposta deve ser 200
    E a região do alerta deve ter 1 alerta aberto e 480 litros de desperdício estimado
    E o plano de mitigação da região do alerta deve conter "Dispatch field teams"

  @positivo
  Cenário: Alerta resolvido sai do relatório da região
    Dado que existe um alerta de vazamento aberto
    E que o alerta já foi resolvido
    E que estou autenticado como "admin"
    Quando consulto o relatório de impacto hídrico
    Então a região do alerta deve ter 0 alertas abertos e 0 litros de desperdício estimado
    E o plano de mitigação da região do alerta deve conter "maintain monitoring cadence"

  @negativo @governanca
  Cenário: Relatório sem token é bloqueado
    Dado que não estou autenticado
    Quando consulto o relatório de impacto hídrico
    Então o status da resposta deve ser 401
