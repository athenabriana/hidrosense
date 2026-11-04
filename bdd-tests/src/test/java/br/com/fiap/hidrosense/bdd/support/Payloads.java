package br.com.fiap.hidrosense.bdd.support;

import java.util.LinkedHashMap;
import java.util.Map;

/** Request bodies for the API, as maps so a scenario can override one field to test validation. */
public final class Payloads {

    private Payloads() {
    }

    public static Map<String, Object> consumerUnit(String code, String region, double idleFlowThresholdLpm) {
        Map<String, Object> body = new LinkedHashMap<>();
        body.put("code", code);
        body.put("name", "Unidade de teste " + code);
        body.put("type", 0); // ConsumerUnitType.Residential: the API binds this enum by number
        body.put("region", region);
        body.put("idleFlowThresholdLpm", idleFlowThresholdLpm);
        body.put("monthlyBudgetLiters", 15000);
        return body;
    }

    public static Map<String, Object> reading(int consumerUnitId, double volumeLiters, double flowRateLpm) {
        Map<String, Object> body = new LinkedHashMap<>();
        body.put("consumerUnitId", consumerUnitId);
        body.put("volumeLiters", volumeLiters);
        body.put("flowRateLpm", flowRateLpm);
        return body;
    }

    public static Map<String, Object> login(String username, String password) {
        Map<String, Object> body = new LinkedHashMap<>();
        body.put("username", username);
        body.put("password", password);
        return body;
    }
}
