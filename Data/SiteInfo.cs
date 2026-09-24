namespace CarsiDekor.Web.Data;

/// <summary>
/// Sitenin her yerinde kullanılan iletişim bilgileri.
/// Değiştirmek istediğin bir bilgi olursa sadece bu dosyayı düzenlemen yeterli.
/// </summary>
public static class SiteInfo
{
    public const string Name = "Çarşı Dekor";

    // Ekranda görünen telefon
    public const string Phone = "+905327130245";

    // Tıklayınca arama başlatan format: + ve ülke kodu ile, boşluksuz
    public const string PhoneLink = "05327130245";

    // WhatsApp numarası: başında + olmadan, ülke koduyla (90 ile başlar)
    public const string WhatsApp = "05327130245";

    public const string Email = "carsidekor@gmail.com";

    public const string Address = "Mustafa Kemal Paşa Mah. Çetin Sokak No:103 Avcılar/İstanbul";

    // Haritada aranacak metin. Tam adresi yazarsan pin doğru yere düşer.
    public const string MapQuery = "Çarşı Dekor";

    public const string WorkingHours = "Pazartesi - Cumartesi, 09:00 - 19:30";
}
