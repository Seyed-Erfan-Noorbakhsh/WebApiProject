@echo off
echo Testing Shop API...
echo.

echo Testing Health Endpoint...
curl -s http://localhost:5227/health
echo.
echo.

echo Testing Users Endpoint...
curl -s http://localhost:5227/api/users
echo.
echo.

echo Creating a test user...
curl -s -X POST http://localhost:5227/api/users -H "Content-Type: application/json" -d "{\"fullName\":\"Test User\"}"
echo.
echo.

echo Testing Products Endpoint...
curl -s http://localhost:5227/api/products
echo.
echo.

echo API Test Complete!
pause