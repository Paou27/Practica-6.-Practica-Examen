<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\MultaController;
use App\Http\Controllers\LoginController;
use App\Http\Middleware\JwMiddleware;

Route::post('/login', [LoginController::class, 'login']);

Route::get('/multa', [MultaController::class, 'index']);
Route::get('/multa/{codigo}', [MultaController::class, 'show']);
Route::post('/multa', [MultaController::class, 'store'])->middleware(JwMiddleware::class);
