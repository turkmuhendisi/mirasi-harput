package com.mirasiharput.features.certificate

import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

class CertificateRepository {

    private val api: CertificateApi by lazy {
        val logging = HttpLoggingInterceptor().apply {
            level = if (ApiConfig.ENABLE_LOGGING) {
                HttpLoggingInterceptor.Level.BODY
            } else {
                HttpLoggingInterceptor.Level.NONE
            }
        }

        val client = OkHttpClient.Builder()
            .connectTimeout(20, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .addInterceptor(logging)
            .build()

        Retrofit.Builder()
            .baseUrl(ApiConfig.BASE_URL)
            .client(client)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
            .create(CertificateApi::class.java)
    }

    suspend fun requestCertificate(fullName: String, email: String): Result<String> {
        return try {
            val response = api.sendCertificate(
                CertificateRequest(
                    fullName = fullName.trim(),
                    email = email.trim(),
                    apiKey = ApiConfig.API_KEY,
                ),
            )
            if (response.success) {
                Result.success(
                    response.message ?: "Sertifikanız e-posta adresinize gönderildi.",
                )
            } else {
                Result.failure(
                    CertificateException(
                        response.message ?: "Sertifika gönderilemedi. Lütfen tekrar deneyin.",
                    ),
                )
            }
        } catch (e: Exception) {
            Result.failure(
                CertificateException(
                    "Bağlantı hatası. İnternet bağlantınızı kontrol edip tekrar deneyin.",
                    e,
                ),
            )
        }
    }
}

class CertificateException(
    override val message: String,
    cause: Throwable? = null,
) : Exception(message, cause)
