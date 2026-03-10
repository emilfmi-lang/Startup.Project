using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Interfaces;
using Startup_Project.Infrastructure.Concretes.Services;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/mock-payment")]
[ApiController]
[AllowAnonymous]
public class MockPaymentController(
    IConfiguration configuration,
    IApplicationDbContext context) : ControllerBase
{
    [HttpGet("checkout")]
    public IActionResult Checkout([FromQuery] string sessionId)
    {
        if (!configuration.GetValue<bool>("KapitalBank:UseMock"))
            return NotFound();

        var session = MockBankService.GetSession(sessionId);

        if (session is not null)
            return Content(CheckoutPage.Build(session.SessionId, session.Amount, session.Description), "text/html");

        // API restart olduqda in-memory session itirilir.
        // DB-dan öd?ni? m?lumat?n? oxuyub checkout page yarad?r?q.
            var sessionPayment = context.SessionPayments
                .FirstOrDefault(p => p.BankSessionId == sessionId);

            if (sessionPayment is not null)
            {
                MockBankService.RecoverSession(sessionId, sessionPayment.Amount,
                    "FitZone Session", "/api/session-payment/callback");
                return Content(CheckoutPage.Build(sessionId, sessionPayment.Amount, "FitZone Session"), "text/html");
            }

            var packagePayment = context.Payments
                .FirstOrDefault(p => p.SessionId == sessionId);

            if (packagePayment is not null)
            {
                MockBankService.RecoverSession(sessionId, packagePayment.Amount,
                    "FitZone Package", "/api/payment/package-callback");
                return Content(CheckoutPage.Build(sessionId, packagePayment.Amount, "FitZone Package"), "text/html");
            }

            return NotFound("Sessiya tap?lmad?.");
        }

        [HttpPost("process")]
    public IActionResult Process(
        [FromForm] string sessionId,
        [FromForm] string action,
        [FromForm] string? cardNumber,
        [FromForm] string? expiry,
        [FromForm] string? cvv,
        [FromForm] string? cardHolder)
    {
        if (!configuration.GetValue<bool>("KapitalBank:UseMock"))
            return NotFound();

        var session = MockBankService.GetSession(sessionId);

        if (session is not null)
        {
            if (action == "approve")
            {
                if (!string.IsNullOrEmpty(cardNumber))
                    MockBankService.SetCardInfo(sessionId, cardNumber, cardHolder ?? "TEST USER");

                MockBankService.ApproveSession(sessionId);
            }
            else
            {
                MockBankService.DeclineSession(sessionId);
            }

            var callbackPath = session.CallbackPath ?? "/api/payment/package-callback";
            return Redirect(callbackPath + "?sessionId=" + sessionId);
        }

        // API restart olduqda in-memory session itirilir.
        // SessionPayments c?dv?lind?n BankSessionId il? callback path-? mü?yy?n edirik.
        var isSessionPayment = context.SessionPayments
            .Any(p => p.BankSessionId == sessionId);

        var fallbackCallback = isSessionPayment
            ? "/api/session-payment/callback"
            : "/api/payment/package-callback";

        return Redirect(fallbackCallback + "?sessionId=" + sessionId);
    }

    [HttpGet("3dsecure")]
    public IActionResult ThreeDSecure([FromQuery] string sessionId)
    {
        if (!configuration.GetValue<bool>("KapitalBank:UseMock"))
            return NotFound();

        var session = MockBankService.GetSession(sessionId);
        if (session is null)
            return NotFound();

        return Content(ThreeDSecurePage.Build(session.SessionId, session.Amount), "text/html");
    }

    [HttpPost("3dsecure-verify")]
    public IActionResult ThreeDSecureVerify([FromForm] string sessionId, [FromForm] string otp)
    {
        if (!configuration.GetValue<bool>("KapitalBank:UseMock"))
            return NotFound();

        var session = MockBankService.GetSession(sessionId);
        if (session is null)
            return NotFound();

        if (otp is "123456" or "000000")
        {
            MockBankService.ApproveSession(sessionId);
            var callbackPath = session.CallbackPath ?? "/api/payment/package-callback";
            return Redirect(callbackPath + "?sessionId=" + sessionId);
        }

        return Content(ThreeDSecurePage.BuildError(session.SessionId, session.Amount), "text/html");
    }
}

#region ==================== CHECKOUT PAGE ====================

internal static class CheckoutPage
{
    internal static string Build(string sessionId, decimal amount, string description)
    {
        var sid = System.Net.WebUtility.HtmlEncode(sessionId);
        var desc = System.Net.WebUtility.HtmlEncode(description);
        var amt = amount.ToString("F2");

        return @"<!DOCTYPE html>
<html lang=""az"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<title>Öd?ni? — KapitalBank Sandbox</title>
<style>
*{margin:0;padding:0;box-sizing:border-box}
body{font-family:'Segoe UI',system-ui,sans-serif;background:#f0f2f5;min-height:100vh;display:flex;align-items:center;justify-content:center}
.wrap{width:480px}
.bank-header{background:#003d7a;padding:20px 30px;border-radius:16px 16px 0 0;display:flex;align-items:center;justify-content:space-between}
.bank-logo{color:#fff;font-size:20px;font-weight:700;letter-spacing:0.5px}
.bank-logo span{color:#f7c948}
.secure-badge{background:rgba(255,255,255,0.15);color:#fff;padding:4px 12px;border-radius:12px;font-size:11px}
.card{background:#fff;padding:32px;border-radius:0 0 16px 16px;box-shadow:0 8px 32px rgba(0,0,0,0.12)}
.merchant-info{background:#f8f9fa;border-radius:10px;padding:16px;margin-bottom:24px;display:flex;justify-content:space-between;align-items:center}
.merchant-name{color:#666;font-size:13px}
.merchant-amount{font-size:28px;font-weight:700;color:#003d7a}
.merchant-currency{font-size:14px;color:#666;font-weight:400}
.field{margin-bottom:18px;position:relative}
.field label{display:block;color:#555;font-size:13px;margin-bottom:6px;font-weight:500}
.field input{width:100%;padding:14px 16px;background:#f8f9fa;border:2px solid #e0e0e0;border-radius:10px;font-size:16px;color:#333;outline:none;transition:border-color 0.2s}
.field input:focus{border-color:#003d7a;background:#fff}
.field input.error{border-color:#e53935}
.field .card-icon{position:absolute;right:14px;top:34px;font-size:20px}
.row{display:flex;gap:14px}
.row .field{flex:1}
.error-msg{color:#e53935;font-size:12px;margin-top:4px;display:none}
.btn{width:100%;padding:16px;border:none;border-radius:10px;font-size:16px;font-weight:600;cursor:pointer;transition:all 0.2s;margin-bottom:10px}
.btn:disabled{opacity:0.5;cursor:not-allowed}
.btn-pay{background:#003d7a;color:#fff}
.btn-pay:hover:not(:disabled){background:#002d5c}
.btn-cancel{background:#f8f9fa;color:#e53935;border:1px solid #e0e0e0}
.btn-cancel:hover{background:#fee}
.footer{text-align:center;padding:16px;color:#999;font-size:11px}
.footer img{height:16px;vertical-align:middle}
.spinner{display:none;text-align:center;padding:40px}
.spinner .dot{display:inline-block;width:10px;height:10px;border-radius:50%;background:#003d7a;margin:0 4px;animation:bounce 1.4s infinite ease-in-out both}
.spinner .dot:nth-child(1){animation-delay:-0.32s}
.spinner .dot:nth-child(2){animation-delay:-0.16s}
@keyframes bounce{0%,80%,100%{transform:scale(0)}40%{transform:scale(1)}}
.spinner-text{color:#666;margin-top:16px;font-size:14px}
</style>
</head>
<body>
<div class=""wrap"">
<div class=""bank-header"">
<div class=""bank-logo"">Kapital<span>Bank</span></div>
<div class=""secure-badge"">?? SSL Secure</div>
</div>
<div class=""card"">
<div id=""formArea"">
<div class=""merchant-info"">
<div><div class=""merchant-name"">" + desc + @"</div></div>
<div class=""merchant-amount"">" + amt + @" <span class=""merchant-currency"">AZN</span></div>
</div>
<form id=""payForm"" action=""/api/mock-payment/process"" method=""post"" onsubmit=""return validateAndSubmit()"">
<input type=""hidden"" name=""sessionId"" value=""" + sid + @"""/>
<input type=""hidden"" name=""action"" value=""approve""/>
<div class=""field"">
<label>Kart nömr?si</label>
<input type=""text"" id=""cardNumber"" name=""cardNumber"" placeholder=""0000 0000 0000 0000"" maxlength=""19"" autocomplete=""cc-number"" oninput=""formatCard(this)""/>
<span class=""card-icon"" id=""cardIcon""></span>
<div class=""error-msg"" id=""cardError"">Düzgün kart nömr?si daxil edin</div>
</div>
<div class=""field"">
<label>Kart sahibi</label>
<input type=""text"" id=""cardHolder"" name=""cardHolder"" placeholder=""AD SOYAD"" autocomplete=""cc-name"" style=""text-transform:uppercase""/>
</div>
<div class=""row"">
<div class=""field"">
<label>Son tarix</label>
<input type=""text"" id=""expiry"" name=""expiry"" placeholder=""AA/??"" maxlength=""5"" autocomplete=""cc-exp"" oninput=""formatExpiry(this)""/>
<div class=""error-msg"" id=""expiryError"">Keç?rsiz tarix</div>
</div>
<div class=""field"">
<label>CVV</label>
<input type=""password"" id=""cvv"" name=""cvv"" placeholder=""•••"" maxlength=""4"" autocomplete=""cc-csc""/>
<div class=""error-msg"" id=""cvvError"">CVV laz?md?r</div>
</div>
</div>
<button type=""submit"" class=""btn btn-pay"" id=""payBtn"">Öd? " + amt + @" AZN</button>
</form>
<form action=""/api/mock-payment/process"" method=""post"">
<input type=""hidden"" name=""sessionId"" value=""" + sid + @"""/>
<input type=""hidden"" name=""action"" value=""decline""/>
<button type=""submit"" class=""btn btn-cancel"">L??v et</button>
</form>
</div>
<div class=""spinner"" id=""spinner"">
<div><span class=""dot""></span><span class=""dot""></span><span class=""dot""></span></div>
<div class=""spinner-text"">Öd?ni? emal olunur...</div>
</div>
</div>
<div class=""footer"">
Sandbox test mühiti · Heç bir real ?m?liyyat apar?lm?r<br/>
Session: " + sid + @"
</div>
</div>
<script>
function formatCard(el){
  var v=el.value.replace(/\D/g,'');
  el.value=v.replace(/(\d{4})(?=\d)/g,'$1 ').trim();
  var icon=document.getElementById('cardIcon');
  if(v.startsWith('4'))icon.textContent='?? Visa';
  else if(v.startsWith('5'))icon.textContent='?? MC';
  else if(v.startsWith('3'))icon.textContent='?? Amex';
  else icon.textContent='';
}
function formatExpiry(el){
  var v=el.value.replace(/\D/g,'');
  if(v.length>=2)el.value=v.substring(0,2)+'/'+v.substring(2,4);
  else el.value=v;
}
function validateAndSubmit(){
  var ok=true;
  var cn=document.getElementById('cardNumber').value.replace(/\s/g,'');
  var exp=document.getElementById('expiry').value;
  var cvv=document.getElementById('cvv').value;
  document.querySelectorAll('.error-msg').forEach(function(e){e.style.display='none'});
  document.querySelectorAll('input').forEach(function(e){e.classList.remove('error')});
  if(cn.length<13||!luhn(cn)){
    document.getElementById('cardError').style.display='block';
    document.getElementById('cardNumber').classList.add('error');
    ok=false;
  }
  if(!/^\d{2}\/\d{2}$/.test(exp)){
    document.getElementById('expiryError').style.display='block';
    document.getElementById('expiry').classList.add('error');
    ok=false;
  }
  if(cvv.length<3){
    document.getElementById('cvvError').style.display='block';
    document.getElementById('cvv').classList.add('error');
    ok=false;
  }
  if(ok){
    document.getElementById('formArea').style.display='none';
    document.getElementById('spinner').style.display='block';
  }
  return ok;
}
function luhn(n){
  var s=0,b=false;
  for(var i=n.length-1;i>=0;i--){
    var d=parseInt(n[i]);
    if(b&&(d*=2)>9)d-=9;
    s+=d;b=!b;
  }
  return s%10===0;
}
</script>
</body>
</html>";
    }
}

#endregion

#region ==================== 3D SECURE PAGE ====================

internal static class ThreeDSecurePage
{
    internal static string Build(string sessionId, decimal amount)
    {
        var sid = System.Net.WebUtility.HtmlEncode(sessionId);
        return @"<!DOCTYPE html>
<html lang=""az"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<title>3D Secure — T?sdiq</title>
<style>
*{margin:0;padding:0;box-sizing:border-box}
body{font-family:'Segoe UI',sans-serif;background:#f0f2f5;min-height:100vh;display:flex;align-items:center;justify-content:center}
.card{background:#fff;border-radius:16px;padding:40px;width:400px;box-shadow:0 8px 32px rgba(0,0,0,0.1);text-align:center}
.shield{font-size:48px;margin-bottom:16px}
h2{color:#333;margin-bottom:8px}
.sub{color:#888;font-size:14px;margin-bottom:24px}
.otp-input{width:200px;padding:14px;font-size:24px;text-align:center;border:2px solid #e0e0e0;border-radius:10px;letter-spacing:8px;margin:0 auto 20px;display:block}
.otp-input:focus{outline:none;border-color:#003d7a}
.btn{width:200px;padding:14px;background:#003d7a;color:#fff;border:none;border-radius:10px;font-size:16px;font-weight:600;cursor:pointer}
.hint{color:#aaa;font-size:12px;margin-top:16px}
</style>
</head>
<body>
<div class=""card"">
<div class=""shield"">???</div>
<h2>3D Secure T?sdiqi</h2>
<div class=""sub"">Bank?n?z t?r?find?n gönd?ril?n kodu daxil edin</div>
<form action=""/api/mock-payment/3dsecure-verify"" method=""post"">
<input type=""hidden"" name=""sessionId"" value=""" + sid + @"""/>
<input type=""text"" name=""otp"" class=""otp-input"" maxlength=""6"" placeholder=""------"" autofocus/>
<button type=""submit"" class=""btn"">T?sdiql?</button>
</form>
<div class=""hint"">Test kodu: <strong>123456</strong></div>
</div>
</body>
</html>";
    }

    internal static string BuildError(string sessionId, decimal amount)
    {
        var sid = System.Net.WebUtility.HtmlEncode(sessionId);
        return @"<!DOCTYPE html>
<html lang=""az"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<title>3D Secure — X?ta</title>
<style>
*{margin:0;padding:0;box-sizing:border-box}
body{font-family:'Segoe UI',sans-serif;background:#f0f2f5;min-height:100vh;display:flex;align-items:center;justify-content:center}
.card{background:#fff;border-radius:16px;padding:40px;width:400px;box-shadow:0 8px 32px rgba(0,0,0,0.1);text-align:center}
.shield{font-size:48px;margin-bottom:16px}
h2{color:#e53935;margin-bottom:8px}
.sub{color:#888;font-size:14px;margin-bottom:24px}
.otp-input{width:200px;padding:14px;font-size:24px;text-align:center;border:2px solid #e53935;border-radius:10px;letter-spacing:8px;margin:0 auto 20px;display:block}
.otp-input:focus{outline:none;border-color:#003d7a}
.btn{width:200px;padding:14px;background:#003d7a;color:#fff;border:none;border-radius:10px;font-size:16px;font-weight:600;cursor:pointer}
.err{color:#e53935;font-size:13px;margin-bottom:16px}
.hint{color:#aaa;font-size:12px;margin-top:16px}
</style>
</head>
<body>
<div class=""card"">
<div class=""shield"">??</div>
<h2>Kod yanl??d?r</h2>
<div class=""err"">Daxil etdiyiniz kod düzgün deyil. Yenid?n c?hd edin.</div>
<form action=""/api/mock-payment/3dsecure-verify"" method=""post"">
<input type=""hidden"" name=""sessionId"" value=""" + sid + @"""/>
<input type=""text"" name=""otp"" class=""otp-input"" maxlength=""6"" placeholder=""------"" autofocus/>
<button type=""submit"" class=""btn"">Yenid?n c?hd et</button>
</form>
<div class=""hint"">Test kodu: <strong>123456</strong></div>
</div>
</body>
</html>";
    }
}

#endregion
