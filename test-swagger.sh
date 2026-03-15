#!/bin/bash

# Swagger UI Test Script
# This script tests the Swagger UI functionality when the application is running

BASE_URL="http://localhost:5000"
SWAGGER_UI_PATH="/swagger/ui"
SWAGGER_JSON_PATH="/swagger/v1/swagger.json"

echo "Testing Swagger UI functionality..."
echo "=================================="

# Test 1: Swagger UI Index
echo "Test 1: Swagger UI Index Page"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL$SWAGGER_UI_PATH")
if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "301" ]; then
    echo "✅ Swagger UI index returns $HTTP_CODE (success/redirect)"

    # Check if it contains expected content (follow redirects)
    if curl -s -L "$BASE_URL$SWAGGER_UI_PATH" | grep -q "HDB Data Services API"; then
        echo "✅ Swagger UI contains expected title"
    else
        echo "❌ Swagger UI missing expected content"
    fi

    if curl -s -L "$BASE_URL$SWAGGER_UI_PATH" | grep -q "swagger-ui"; then
        echo "✅ Swagger UI contains swagger-ui elements"
    else
        echo "❌ Swagger UI missing swagger-ui elements"
    fi
else
    echo "❌ Swagger UI index failed to load (HTTP $HTTP_CODE)"
fi

echo ""

# Test 2: Swagger JSON
echo "Test 2: Swagger JSON Endpoint"
if curl -s -o /dev/null -w "%{http_code}" "$BASE_URL$SWAGGER_JSON_PATH" | grep -q "200"; then
    echo "✅ Swagger JSON returns 200 OK"

    # Check if it contains expected endpoints
    JSON_CONTENT=$(curl -s "$BASE_URL$SWAGGER_JSON_PATH")

    if echo "$JSON_CONTENT" | grep -q "/sites"; then
        echo "✅ Swagger JSON contains /sites endpoint"
    else
        echo "❌ Swagger JSON missing /sites endpoint"
    fi

    if echo "$JSON_CONTENT" | grep -q "/datatypes"; then
        echo "✅ Swagger JSON contains /datatypes endpoint"
    else
        echo "❌ Swagger JSON missing /datatypes endpoint"
    fi

    if echo "$JSON_CONTENT" | grep -q "/modelruns"; then
        echo "✅ Swagger JSON contains /modelruns endpoint"
    else
        echo "❌ Swagger JSON missing /modelruns endpoint"
    fi

    if echo "$JSON_CONTENT" | grep -q "/series"; then
        echo "✅ Swagger JSON contains /series endpoint"
    else
        echo "❌ Swagger JSON missing /series endpoint"
    fi
else
    echo "❌ Swagger JSON endpoint failed to load"
fi

echo ""
echo "Swagger UI tests completed!"