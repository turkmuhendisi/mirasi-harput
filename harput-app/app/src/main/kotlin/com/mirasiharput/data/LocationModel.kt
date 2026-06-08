package com.mirasiharput.data

data class LocationModel(
    val id: String,
    val title: String,
    val qrPayload: String,
    val description: String,
    val voiceText: String,
    val modelPath: String,
    val audioPath: String,
    val modelTransform: ModelTransform = ModelTransform(),
)
