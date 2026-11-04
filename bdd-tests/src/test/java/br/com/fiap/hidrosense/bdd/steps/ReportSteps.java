package br.com.fiap.hidrosense.bdd.steps;

import br.com.fiap.hidrosense.bdd.support.ApiContext;
import io.cucumber.java.pt.Então;
import io.cucumber.java.pt.Quando;

import java.util.List;
import java.util.Map;

import static org.hamcrest.MatcherAssert.assertThat;
import static org.hamcrest.Matchers.containsString;
import static org.junit.jupiter.api.Assertions.assertEquals;

public class ReportSteps {

    private final ApiContext ctx;

    public ReportSteps(ApiContext ctx) {
        this.ctx = ctx;
    }

    @Quando("consulto o relatório de impacto hídrico")
    public void consultoORelatorioDeImpactoHidrico() {
        ctx.setResponse(ctx.request().get("/api/v1/reports/water-impact"));
    }

    @Então("a região do alerta deve ter {int} alerta(s) aberto(s) e {double} litros de desperdício estimado")
    public void aRegiaoDoAlertaDeveTer(int openAlerts, double wasteLiters) {
        Map<String, Object> region = alertRegion();
        assertEquals(openAlerts, ((Number) region.get("openLeakAlerts")).intValue(), "openLeakAlerts");
        assertEquals(wasteLiters, ((Number) region.get("estimatedWasteLiters")).doubleValue(), 0.01,
                "estimatedWasteLiters");
    }

    @Então("o plano de mitigação da região do alerta deve conter {string}")
    public void oPlanoDeMitigacaoDeveConter(String text) {
        assertThat((String) alertRegion().get("mitigationPlan"), containsString(text));
    }

    private Map<String, Object> alertRegion() {
        List<Map<String, Object>> regions = ctx.response().path("regions");
        return regions.stream()
                .filter(r -> ctx.unitRegion().equals(r.get("region")))
                .findFirst()
                .orElseThrow(() -> new AssertionError("Região " + ctx.unitRegion() + " ausente do relatório"));
    }
}
