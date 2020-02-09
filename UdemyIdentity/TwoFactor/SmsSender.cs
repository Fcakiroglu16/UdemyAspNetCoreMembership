using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UdemyIdentity.Service;

namespace UdemyIdentity.Service
{
    public class SmsSender
    {
        private readonly TwoFactorOptions _twoFactorOptions;

        private readonly TwoFactorService _twoFactorService;

        public SmsSender(IOptions<TwoFactorOptions> options, TwoFactorService twoFactorService)
        {
            _twoFactorOptions = options.Value;
            _twoFactorService = twoFactorService;
        }

        public string Send(string phone)
        {
            string code = _twoFactorService.GetCodeVerification().ToString();

            //SMS PROVIDER CODES

            return "2222";
        }
    }
}