using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.MailDtos;
using IlkProjem.Core.Specifications;
using IlkProjem.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace IlkProjem.BLL.Jobs;

[DisallowConcurrentExecution]
public class DailyMailJob : IJob
{
    private readonly IMailService _mailService;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<DailyMailJob> _logger;

    public DailyMailJob(IMailService mailService, ICustomerRepository customerRepository, ILogger<DailyMailJob> logger)
    {
        _mailService = mailService;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("DailyMailJob başlatıldı: {Time}", DateTime.Now);

        try
        {
            // TODO: Alıcı listesini veritabanından veya ayarlardan çekebilirsiniz.
            // Şimdilik örnek bir gönderim yapıyoruz.
            var mailDto = new MailDto
            {
                To = "admin@example.com", // Bu alan dinamik hale getirilmeli
                Subject = "Günlük Sistem Özeti - " + DateTime.Today.ToString("dd.MM.yyyy"),
                Body = $@"
                    <h1>Günlük Rapor</h1>
                    <p>Merhaba,</p>
                    <p>Sistem her sabah olduğu gibi bugün de başarıyla çalışıyor.</p>
                    <p>Saat: {DateTime.Now:HH:mm:ss}</p>
                    <br/>
                    <p>Bu otomatik bir bilgilendirme mesajıdır.</p>"
            };

            await _mailService.SendMailAsync(mailDto);
            _logger.LogInformation("Genel günlük özet maili başarıyla gönderildi.");

            // --- DOĞUM GÜNÜ MAİLLERİ ---
            var today = DateTime.Today;
            var birthdaySpec = new CustomerBirthdaySpecification(today);
            var bdayCustomers = await _customerRepository.ListAsync(birthdaySpec);

            _logger.LogInformation("Bugün doğum günü olan {Count} müşteri bulundu.", bdayCustomers.Count);

            foreach (var customer in bdayCustomers)
            {
                if (string.IsNullOrEmpty(customer.Email)) continue;

                var bdayMail = new MailDto
                {
                    To = customer.Email,
                    Subject = "İyi ki Varsın! 🎂",
                    Body = $@"
                        <div style='text-align: center; font-family: sans-serif;'>
                            <h1 style='color: #2c3e50;'>Mutlu Yıllar, {customer.Name}! 🎈</h1>
                            <p style='font-size: 1.1em;'>Yeni yaşının sana sağlık, mutluluk ve bol kazanç getirmesini dileriz.</p>
                            <p style='color: #7f8c8d;'>BankApp ailesi olarak her zaman yanındayız.</p>
                            <br/>
                            <hr style='border: 0; border-top: 1px solid #eee;'/>
                            <p style='font-size: 0.8em; color: #bdc3c7;'>Bu bir doğum günü tebriği mesajıdır. / By BankApp</p>
                        </div>"
                };

                try 
                {
                    await _mailService.SendMailAsync(bdayMail);
                    _logger.LogInformation("{Email} adresine doğum günü maili gönderildi.", customer.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Email} adresine doğum günü maili gönderilirken hata oluştu.", customer.Email);
                }
            }

            _logger.LogInformation("DailyMailJob başarıyla tamamlandı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DailyMailJob yürütülürken bir hata oluştu.");
        }
    }
}
