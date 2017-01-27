using Microsoft.Owin.Security;
using System;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using LoginTest.App_Start;
using System.Linq;

namespace LoginTest.Controllers
{
    public class AdAuthenticationService
    {
        public class AuthenticationResult
        {
            public AuthenticationResult(string errorMessage = null)
            {
                ErrorMessage = errorMessage;
            }

            public String ErrorMessage { get; private set; }
            public Boolean IsSuccess => String.IsNullOrEmpty(ErrorMessage);
        }

        private readonly IAuthenticationManager authenticationManager;

        public AdAuthenticationService(IAuthenticationManager authenticationManager)
        {
            this.authenticationManager = authenticationManager;
        }
        public AuthenticationResult SignIn(String username, String password)
        {
            ContextType authenticationType = ContextType.Domain;
            var DBContext = new DBEntities();
            var user = from USUARIOS in DBContext.USUARIOS where (USUARIOS.USUARIO == username) select USUARIOS;
            USUARIOS u = new USUARIOS();
            AUDITORIAS au = new AUDITORIAS();
            if (user.Any())
            {
                foreach (var iuser in user)
                {
                    u = iuser;
                }
            }
            else
            {
                u.CONTRASENA = password;
                u.USUARIO = username;
                u.IDROLE = 2;
                DBContext.USUARIOS.Add(u);
                DBContext.SaveChanges();
            }
            au.USUARIOS = u;
            au.ACCION = "LDAP_REQUEST";
            au.TIMESTAMP = DateTime.Now;
            DBContext.AUDITORIAS.Add(au);
            DBContext.SaveChanges();

            PrincipalContext principalContext = new PrincipalContext(authenticationType);
            bool isAuthenticated = false;
            UserPrincipal userPrincipal = new UserPrincipal(principalContext);
            userPrincipal.SamAccountName = username;
            var searcher = new PrincipalSearcher(userPrincipal);
            try
            {
                isAuthenticated = principalContext.ValidateCredentials(username, password, ContextOptions.Negotiate);
                au = new AUDITORIAS();
                au.USUARIOS = u;
                au.ACCION = "LDAP_CONNECT";
                au.TIMESTAMP = DateTime.Now;
                DBContext.AUDITORIAS.Add(au);
                DBContext.SaveChanges();
                if (isAuthenticated)
                {
                    userPrincipal = searcher.FindOne() as UserPrincipal;
                }
            }
            catch (Exception)
            {
                au = new AUDITORIAS();
                au.USUARIOS = u;
                au.ACCION = "LDAP_FAILED";
                au.TIMESTAMP = DateTime.Now;
                DBContext.AUDITORIAS.Add(au);
                DBContext.SaveChanges();
                isAuthenticated = false;
                userPrincipal = null;
            }

            if (!isAuthenticated || userPrincipal == null)
            {
                au = new AUDITORIAS();
                au.USUARIOS = u;
                au.ACCION = "LDAP_BADLOGIN";
                au.TIMESTAMP = DateTime.Now;
                DBContext.AUDITORIAS.Add(au);
                DBContext.SaveChanges();
                return new AuthenticationResult("Usuario o Contraseña incorrectos");
            }

            if (userPrincipal.IsAccountLockedOut())
            {
                au = new AUDITORIAS();
                au.USUARIOS = u;
                au.ACCION = "LDAP_LOCKED";
                au.TIMESTAMP = DateTime.Now;
                DBContext.AUDITORIAS.Add(au);
                DBContext.SaveChanges();
                return new AuthenticationResult("Cuenta bloqueada");
            }

            if (userPrincipal.Enabled.HasValue && userPrincipal.Enabled.Value == false)
            {
                au = new AUDITORIAS();
                au.USUARIOS = u;
                au.ACCION = "LDAP_DISABLED";
                au.TIMESTAMP = DateTime.Now;
                DBContext.AUDITORIAS.Add(au);
                DBContext.SaveChanges();
                return new AuthenticationResult("Cuenta deshabilitada");
            }
            au = new AUDITORIAS();
            au.USUARIOS = u;
            au.ACCION = "LDAP_SUCCESS";
            au.TIMESTAMP = DateTime.Now;
            DBContext.AUDITORIAS.Add(au);
            DBContext.SaveChanges();
            var identity = CreateIdentity(userPrincipal);

            authenticationManager.SignOut(MyAuthentication.ApplicationCookie);
            authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = false }, identity);


            return new AuthenticationResult();
        }


        private ClaimsIdentity CreateIdentity(UserPrincipal userPrincipal)
        {
            var identity = new ClaimsIdentity(MyAuthentication.ApplicationCookie, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Active Directory"));
            identity.AddClaim(new Claim(ClaimTypes.Name, userPrincipal.SamAccountName));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userPrincipal.SamAccountName));
            if (!String.IsNullOrEmpty(userPrincipal.EmailAddress))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, userPrincipal.EmailAddress));
            }
            return identity;
        }
    }
}