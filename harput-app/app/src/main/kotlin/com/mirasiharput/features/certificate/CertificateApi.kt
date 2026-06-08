package com.mirasiharput.features.certificate

import retrofit2.http.Body
import retrofit2.http.POST

data class CertificateRequest(
    val fullName: String,
    val email: String,
    val apiKey: String,
)

data class CertificateResponse(
    val success: Boolean,
    val message: String?,
    val certificateCode: String?,
)

interface CertificateApi {
    @POST("certificate.php")
    suspend fun sendCertificate(@Body request: CertificateRequest): CertificateResponse
}
