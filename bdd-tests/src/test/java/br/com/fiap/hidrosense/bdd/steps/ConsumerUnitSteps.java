package br.com.fiap.hidrosense.bdd.steps;

import br.com.fiap.hidrosense.bdd.support.ApiContext;
import br.com.fiap.hidrosense.bdd.support.Payloads;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import io.cucumber.java.pt.Dado;
import io.cucumber.java.pt.Então;
import io.cucumber.java.pt.Quando;

import java.util.Map;

import static org.hamcrest.Matchers.equalTo;

public class ConsumerUnitSteps {

    private static final String PATH = "/api/v1/consumer-units";
    private static final ObjectMapper JSON = new ObjectMapper();

    private final ApiContext ctx;
    private String sentCode;

    public ConsumerUnitSteps(ApiContext ctx) {
        this.ctx = ctx;
    }

    @Dado("que existe uma unidade consumidora com vazão ociosa de {double} litros por minuto")
    public void queExisteUmaUnidadeConsumidora(double idleFlow) {
        ctx.createUnit(ApiContext.uniqueRegion(), idleFlow);
    }

    @Quando("cadastro uma unidade consumidora na região {string} com vazão ociosa de {double} litros por minuto")
    public void cadastroUmaUnidadeConsumidora(String region, double idleFlow) {
        sentCode = ApiContext.uniqueCode();
        ctx.setResponse(ctx.request().body(Payloads.consumerUnit(sentCode, region, idleFlow)).post(PATH));
    }

    @Quando("cadastro outra unidade consumidora com o mesmo código")
    public void cadastroOutraUnidadeComOMesmoCodigo() {
        ctx.setResponse(ctx.request()
                .body(Payloads.consumerUnit(ctx.unitCode(), "Zona Leste", 1.0))
                .post(PATH));
    }

    /** The value comes as a JSON literal ("" , 0, -1) so one outline covers strings and numbers. */
    @Quando("cadastro uma unidade consumidora com o campo {string} valendo {}")
    public void cadastroComCampoValendo(String field, String jsonValue) throws JsonProcessingException {
        Map<String, Object> body = Payloads.consumerUnit(ApiContext.uniqueCode(), "Zona Sul", 1.0);
        body.put(field, JSON.readValue(jsonValue, Object.class));
        ctx.setResponse(ctx.request().body(body).post(PATH));
    }

    @Então("a unidade retornada deve ter o código enviado")
    public void aUnidadeRetornadaDeveTerOCodigoEnviado() {
        ctx.response().then().body("code", equalTo(sentCode));
    }
}
