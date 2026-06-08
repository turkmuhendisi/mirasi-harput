package com.mirasiharput.features.certificate

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.CheckCircle
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.mirasiharput.ui.components.HeritageBackground
import com.mirasiharput.ui.components.HeritagePrimaryButton
import com.mirasiharput.ui.components.OrnamentDivider
import com.mirasiharput.ui.theme.CertificateGreen
import com.mirasiharput.ui.theme.HeritageBrown
import com.mirasiharput.ui.theme.HeritageBrownSoft
import com.mirasiharput.ui.theme.HeritageGold
import com.mirasiharput.ui.theme.ParchmentLight

@Composable
fun CertificateClaimScreen(
    uiState: CertificateUiState,
    onSubmit: (fullName: String, email: String) -> Unit,
    onBack: () -> Unit,
    onDone: () -> Unit,
    modifier: Modifier = Modifier,
) {
    HeritageBackground(modifier = modifier) {
        Box(modifier = Modifier.fillMaxSize()) {
            IconButton(
                onClick = onBack,
                modifier = Modifier
                    .align(Alignment.TopStart)
                    .padding(12.dp)
                    .size(48.dp)
                    .background(ParchmentLight, RoundedCornerShape(14.dp)),
            ) {
                Icon(
                    imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                    contentDescription = "Geri",
                    tint = HeritageBrown,
                )
            }

            if (uiState is CertificateUiState.Success) {
                SuccessContent(message = uiState.message, onDone = onDone)
            } else {
                FormContent(uiState = uiState, onSubmit = onSubmit)
            }
        }
    }
}

@Composable
private fun FormContent(
    uiState: CertificateUiState,
    onSubmit: (String, String) -> Unit,
) {
    var fullName by rememberSaveable { mutableStateOf("") }
    var email by rememberSaveable { mutableStateOf("") }
    val isSubmitting = uiState is CertificateUiState.Submitting

    Column(
        modifier = Modifier
            .fillMaxSize()
            .verticalScroll(rememberScrollState())
            .padding(horizontal = 28.dp, vertical = 72.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
    ) {
        Text(
            text = "Katılım Sertifikası",
            fontFamily = FontFamily.Serif,
            fontWeight = FontWeight.Bold,
            fontSize = 28.sp,
            color = HeritageBrown,
            textAlign = TextAlign.Center,
        )
        Spacer(modifier = Modifier.height(14.dp))
        OrnamentDivider()
        Spacer(modifier = Modifier.height(14.dp))
        Text(
            text = "Sertifikanızı e-posta ile gönderebilmemiz için bilgilerinizi girin.",
            fontFamily = FontFamily.Serif,
            fontSize = 15.sp,
            lineHeight = 22.sp,
            color = HeritageBrownSoft,
            textAlign = TextAlign.Center,
        )

        Spacer(modifier = Modifier.height(32.dp))

        HeritageTextField(
            value = fullName,
            onValueChange = { fullName = it },
            label = "Ad Soyad",
            keyboardType = KeyboardType.Text,
            enabled = !isSubmitting,
        )

        Spacer(modifier = Modifier.height(16.dp))

        HeritageTextField(
            value = email,
            onValueChange = { email = it },
            label = "E-posta Adresi",
            keyboardType = KeyboardType.Email,
            enabled = !isSubmitting,
        )

        if (uiState is CertificateUiState.Error) {
            Spacer(modifier = Modifier.height(16.dp))
            Text(
                text = uiState.message,
                fontFamily = FontFamily.Serif,
                fontSize = 14.sp,
                color = androidx.compose.ui.graphics.Color(0xFFB23A3A),
                textAlign = TextAlign.Center,
            )
        }

        Spacer(modifier = Modifier.height(28.dp))

        if (isSubmitting) {
            CircularProgressIndicator(color = HeritageGold)
            Spacer(modifier = Modifier.height(12.dp))
            Text(
                text = "Sertifikanız hazırlanıyor...",
                fontFamily = FontFamily.Serif,
                fontSize = 14.sp,
                color = HeritageBrownSoft,
            )
        } else {
            HeritagePrimaryButton(
                text = "Sertifikamı Gönder",
                onClick = { onSubmit(fullName, email) },
                enabled = fullName.isNotBlank() && email.isNotBlank(),
            )
        }
    }
}

@Composable
private fun SuccessContent(message: String, onDone: () -> Unit) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(horizontal = 28.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center,
    ) {
        Icon(
            imageVector = Icons.Filled.CheckCircle,
            contentDescription = null,
            tint = CertificateGreen,
            modifier = Modifier.size(72.dp),
        )
        Spacer(modifier = Modifier.height(20.dp))
        Text(
            text = "Sertifikanız Gönderildi",
            fontFamily = FontFamily.Serif,
            fontWeight = FontWeight.Bold,
            fontSize = 24.sp,
            color = HeritageBrown,
            textAlign = TextAlign.Center,
        )
        Spacer(modifier = Modifier.height(12.dp))
        Text(
            text = message,
            fontFamily = FontFamily.Serif,
            fontSize = 15.sp,
            lineHeight = 22.sp,
            color = HeritageBrownSoft,
            textAlign = TextAlign.Center,
        )
        Spacer(modifier = Modifier.height(32.dp))
        HeritagePrimaryButton(
            text = "Ana Sayfaya Dön",
            onClick = onDone,
        )
    }
}

@Composable
private fun HeritageTextField(
    value: String,
    onValueChange: (String) -> Unit,
    label: String,
    keyboardType: KeyboardType,
    enabled: Boolean,
) {
    OutlinedTextField(
        value = value,
        onValueChange = onValueChange,
        label = { Text(label, fontFamily = FontFamily.Serif) },
        singleLine = true,
        enabled = enabled,
        keyboardOptions = KeyboardOptions(keyboardType = keyboardType),
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(14.dp),
        colors = OutlinedTextFieldDefaults.colors(
            focusedBorderColor = HeritageGold,
            unfocusedBorderColor = HeritageBrownSoft,
            focusedLabelColor = HeritageGold,
            unfocusedLabelColor = HeritageBrownSoft,
            focusedTextColor = HeritageBrown,
            unfocusedTextColor = HeritageBrown,
            cursorColor = HeritageGold,
            focusedContainerColor = ParchmentLight,
            unfocusedContainerColor = ParchmentLight,
        ),
    )
}
