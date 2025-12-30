import os
from locust import HttpUser, task, between


class CoinUpUser(HttpUser):
    wait_time = between(0.2, 1.0)

    def on_start(self):
        self.token = None

        email = os.getenv("EMAIL", "")
        password = os.getenv("PASSWORD", "")

        if not email or not password:
            return

        with self.client.post(
            "/api/auth/login",
            json={"email": email, "password": password},
            catch_response=True,
        ) as resp:
            if resp.status_code != 200:
                resp.failure(f"login failed: {resp.status_code} {resp.text}")
                return

            try:
                data = resp.json()
                self.token = data.get("token") or data.get("accessToken")
            except Exception as ex:
                resp.failure(f"invalid login json: {ex}")

    @task(5)
    def get_coins(self):
        headers = {}
        if self.token:
            headers["Authorization"] = f"Bearer {self.token}"

        self.client.get("/api/coins?page=1&pageSize=20", headers=headers, name="GET /api/coins")
