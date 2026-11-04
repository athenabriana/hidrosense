package br.com.fiap.hidrosense.bdd.steps;

import br.com.fiap.hidrosense.bdd.support.ApiContext;
import io.cucumber.java.pt.Dado;
import io.cucumber.java.pt.Quando;

import java.util.HashMap;
import java.util.Map;

public class LeakAlertSteps {

    private static final String PATH = "/api/v1/leak-alerts";

    private final ApiContext ctx;

    public LeakAlertSteps(ApiContext ctx) {
        this.ctx = ctx;
    }

    @Dado("que existe um alerta de vazamento aberto")
    public void queExisteUmAlertaDeVazamentoAberto() {
        ctx.createOpenAlert(ApiContext.uniqueRegion());
    }

    @Dado("que o alerta já foi resolvido")
    public void queOAlertaJaFoiResolvido() {
        ctx.asAdmin().body(resolveBody("Resolvido na preparação do cenário", false))
                .patch(PATH + "/" + ctx.alertId() + "/resolve")
                .then().statusCode(200);
    }

    @Quando("consulto os alertas abertos da região do alerta")
    public void consultoOsAlertasAbertosDaRegiao() {
        ctx.setResponse(ctx.request()
                .queryParam("status", "Open")
                .queryParam("region", ctx.unitRegion())
                .get(PATH));
    }

    @Quando("resolvo o alerta com a nota {string}")
    public void resolvoOAlertaComANota(String note) {
        resolve(ctx.alertId(), resolveBody(note, false));
    }

    @Quando("descarto o alerta como falso positivo")
    public void descartoOAlertaComoFalsoPositivo() {
        resolve(ctx.alertId(), resolveBody("Falso positivo: manobra de rede programada", true));
    }

    @Quando("tento resolver um alerta inexistente")
    public void tentoResolverUmAlertaInexistente() {
        resolve(999_999_999L, resolveBody("Não existe", false));
    }

    private void resolve(long alertId, Map<String, Object> body) {
        ctx.setResponse(ctx.request().body(body).patch(PATH + "/" + alertId + "/resolve"));
    }

    private static Map<String, Object> resolveBody(String note, boolean dismiss) {
        Map<String, Object> body = new HashMap<>();
        body.put("note", note);
        body.put("dismiss", dismiss);
        return body;
    }
}
