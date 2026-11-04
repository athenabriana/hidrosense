package br.com.fiap.hidrosense.bdd.steps;

import br.com.fiap.hidrosense.bdd.support.ApiContext;
import br.com.fiap.hidrosense.bdd.support.Payloads;
import io.cucumber.java.pt.Dado;
import io.cucumber.java.pt.Então;
import io.cucumber.java.pt.Quando;

import static org.hamcrest.Matchers.equalTo;
import static org.hamcrest.Matchers.everyItem;

public class ConsumptionSteps {

    private static final String PATH = "/api/v1/consumption/readings";

    private final ApiContext ctx;

    public ConsumptionSteps(ApiContext ctx) {
        this.ctx = ctx;
    }

    @Dado("que a unidade já recebeu uma leitura de {double} litros com vazão de {double} litros por minuto")
    public void queAUnidadeJaRecebeuUmaLeitura(double volume, double flow) {
        ctx.postReading(ctx.unitId(), volume, flow).then().statusCode(201);
    }

    @Quando("o hidrômetro envia uma leitura de {double} litros com vazão de {double} litros por minuto")
    public void oHidrometroEnviaUmaLeitura(double volume, double flow) {
        ctx.setResponse(ctx.request().body(Payloads.reading(ctx.unitId(), volume, flow)).post(PATH));
    }

    @Quando("o hidrômetro envia uma leitura para a unidade {int}")
    public void oHidrometroEnviaUmaLeituraParaAUnidade(int unitId) {
        ctx.setResponse(ctx.request().body(Payloads.reading(unitId, 100, 1.0)).post(PATH));
    }

    @Quando("consulto as leituras da unidade")
    public void consultoAsLeiturasDaUnidade() {
        ctx.setResponse(ctx.request().queryParam("consumerUnitId", ctx.unitId()).get(PATH));
    }

    @Então("todas as leituras devem ser da unidade consultada")
    public void todasAsLeiturasDevemSerDaUnidadeConsultada() {
        ctx.response().then()
                .body("items.consumerUnitId", everyItem(equalTo(ctx.unitId())))
                .body("items.consumerUnitCode", everyItem(equalTo(ctx.unitCode())));
    }
}
