package com.mirasiharput.features.quiz

data class QuizQuestion(
    val text: String,
    val options: List<String>,
    val correctIndex: Int,
    val explanation: String,
)
