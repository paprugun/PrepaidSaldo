﻿using ApplicationAuth.Common.Constants;
using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Domain.Entities.Saldo;
using ApplicationAuth.Models.ResponseModels.Saldo;
using ApplicationAuth.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApplicationAuth.Services.Services
{
    public class SaldoService : ISaldoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SaldoService(IUnitOfWork unitOfWork, 
                            IMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;   
        }

        public async Task<string> DeleteSaldo(ApplicationUser user)
        {
            _unitOfWork.Repository<SaldoProfile>().Delete(user.Saldo);
            _unitOfWork.SaveChanges();
            return $"{user.TelegramId}_Saldo було видалено!";
        }


        public async Task<SaldoResponseModel> Get(ApplicationUser user)
        {
            using (IWebDriver driver = new ChromeDriver()) 
            {
                driver.Url = Saldo.Base;
                driver.FindElement(By.XPath("//input[@id='mainform:cardnumber']")).SendKeys(user.Saldo.AccountNumber);
                driver.FindElement(By.XPath("//input[@id='mainform:password']")).SendKeys(user.Saldo.SecureCode);
                driver.FindElement(By.XPath("//a[@href='#'][contains(.,'Next')]")).Click();
                var balance = driver.FindElement(By.XPath("//td[contains(.,'€')]")).GetAttribute("textContent");
                balance = Regex.Replace(balance, @"[ \r\n\t]", "").TrimStart().Replace("€", "").Replace(" ", "");
                balance = Regex.Replace(balance, @"\s+", String.Empty);
                user.Saldo.Balance = double.Parse(balance, CultureInfo.InvariantCulture);
                _unitOfWork.SaveChanges();
                return new SaldoResponseModel() { AccountNumber = user.Saldo.AccountNumber, Balance = double.Parse(balance, CultureInfo.InvariantCulture), Status=true};
            }
        }
    }
}
