# 1. Terraform Provider Tanımları
terraform {
  required_providers {
    google = {
      source  = "hashicorp/google"
      version = "~> 5.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.1"
    }
  }
}

# Makinedeki gcloud credentials'ları ile otomatik çalışır. 
# Proje ID'sini kendi oluşturacağınız GCP proje ID'sine göre güncelleyin.
provider "google" {
  project = "bank-app-bucket" # GCP üzerindeki Proje ID'niz
  region  = "europe-west3" # Frankfurt (Turkiye için en düşük ping gecikmesi)
}

# 2. Bucket İsimlendirmesinde Kullanılacak Rastgele Değer
# GCS Bucket isimlerinin küresel olarak eşsiz (unique) olması zorunludur.
resource "random_string" "suffix" {
  length  = 6
  special = false
  upper   = false
}

# 3. Google Cloud Storage Bucket
resource "google_storage_bucket" "images_bucket" {
  name          = "bankapp-images-${random_string.suffix.result}"
  location      = "EU" # Ücretsiz Tier kapsamına veya bölgesel kurala göre seçebilirsiniz
  storage_class = "STANDARD"

  # Bucket'ın kazara silinmesini önlemek istiyorsanız bu satırı true yapın:
  force_destroy = true 

  # Dışarıdan herkesin okuyabilmesi (Public Read) için gerekli ACL izin modifikasyonlarına hazırlık:
  uniform_bucket_level_access = false

  cors {
    origin          = ["*"]
    method          = ["GET", "OPTIONS"]
    response_header = ["*"]
    max_age_seconds = 3600
  }
}

# 4. Genel (Public) Okuma İzni (Tüm bucket'taki dosyalar browserdan gözükebilir)
resource "google_storage_bucket_iam_member" "public_read" {
  bucket = google_storage_bucket.images_bucket.name
  role   = "roles/storage.objectViewer"
  member = "allUsers"
}

# 5. Outputs
output "gcp_bucket_name" {
  description = "Ayağa Kalkan Geçerli Bucket Adı (Bunu C#'ta kullanacaksınız)"
  value       = google_storage_bucket.images_bucket.name
}
