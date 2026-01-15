# Zeabur 部署配置說明

## MongoDB 連接設定

對於使用 `mongodb+srv://` 協議的 MongoDB Atlas 連接：

### ✅ 正確配置（在 Zeabur 環境變數中設定）

```bash
MongoSettings__ConnectionString=mongodb+srv://<username>:<password>@<cluster-host>/?retryWrites=true&w=majority&appName=<app>
MongoSettings__DatabaseName=portfolio_db
```

### ❌ 不需要設定的參數

~~MongoSettings__UseSsl=true~~  
~~MongoSettings__AllowInsecureSsl=false~~

**原因：** `mongodb+srv://` 協議已經自動啟用 TLS/SSL，手動設定這些參數可能會造成衝突。

## Zeabur 環境變數設定步驟

1. 進入 Zeabur 專案設定
2. 選擇你的服務
3. 點擊 "Environment Variables"（環境變數）
4. 設定以下變數：

| 變數名稱 | 值 |
|---------|-----|
| `MongoSettings__ConnectionString` | `mongodb+srv://<username>:<password>@<cluster-host>/?retryWrites=true&w=majority&appName=<app>` |
| `MongoSettings__DatabaseName` | `portfolio_db` |
| `MongoSettings__MaxConnectionPoolSize` | `100` |
| `MongoSettings__MinConnectionPoolSize` | `10` |
| `MongoSettings__ConnectionTimeout` | `10` |

## 程式碼更新

已修改 `Program.cs` 使其：
- 自動偵測 `mongodb+srv://` 協議
- 對於 SRV 協議，跳過手動 TLS 配置
- 對於 SRV 協議，在 Production 環境不要求設定 `UseSsl` 參數

## 部署後驗證

部署完成後，檢查應用程式日誌是否出現：
```
Now listening on: http://0.0.0.0:8080
Application started
```

測試 API 是否正常：
```bash
curl https://your-app.zeabur.app/api/portfolios/user/67283eee447a55a757f87db8
```
