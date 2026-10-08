#!/bin/bash
echo "Registering patient..."
RESPONSE=$(curl -s -X POST http://localhost:5241/api/auth/register-patient \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser_profile",
    "email": "testuser_profile@example.com",
    "password": "Password123!",
    "fullName": "Test User Profile",
    "dateOfBirth": "1990-01-01"
  }')
echo $RESPONSE
TOKEN=$(echo $RESPONSE | grep -o '"token":"[^"]*' | grep -o '[^"]*$')
PATIENT_ID=$(echo $RESPONSE | grep -o '"patientId":"[^"]*' | grep -o '[^"]*$')

echo "Token: $TOKEN"
echo "Patient ID: $PATIENT_ID"

echo "Fetching profile..."
curl -s -X GET http://localhost:5241/api/PatientProfiles/$PATIENT_ID \
  -H "Authorization: Bearer $TOKEN"
echo ""
