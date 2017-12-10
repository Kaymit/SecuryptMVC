using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using SecuryptMVC.Models;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Security.Principal;

namespace SecuryptMVC
{
    /// <summary>
    /// Email Registration service using SendGrid cloud email service
    /// https://sendgrid.com/solutions/email-api/
    /// </summary>
    /// <author>
    /// Kevin Mitchell 15/11/2017 - 2/12/2017
    /// </author>
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return configSendGridasync(message);
        }

        /// <summary>
        /// Configures an IdentityMessage to be sent by Email service + SendGrid cloud mail server
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <references
        /// >https://docs.microsoft.com/en-us/aspnet/identity/overview/features-api/account-confirmation-and-password-recovery-with-aspnet-identity
        /// </references>
        private Task configSendGridasync(IdentityMessage message)
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY"); 
            var client = new SendGridClient(apiKey);
            var myMessage = new SendGridMessage();
            myMessage.From = new SendGrid.Helpers.Mail.EmailAddress(
                                "admin@techpro2017.com", "Kevin M.");
            myMessage.HtmlContent = message.Body;

            var msg = MailHelper.CreateSingleEmail(
                myMessage.From, 
                SendGrid.Helpers.Mail.MailHelper.StringToEmailAddress(message.Destination), 
                message.Subject, 
                message.Body, 
                message.Body
            );

            return client.SendEmailAsync(msg);
        }
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

	/// <summary>
	/// Template ApplicationRoleManager not included in MS release
	/// https://stackoverflow.com/questions/25417024/no-applicationrolemanager-class-in-my-mvc-5-template
	/// </summary>
	public class ApplicationRoleManager : RoleManager<IdentityRole>
	{
		public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore)
		: base(roleStore) { }

		public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
		{
			var manager = new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<ApplicationDbContext>()));
			return manager;
		}
	}
}
