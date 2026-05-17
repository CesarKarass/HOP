const express = require('express');
const path = require('path');
const { createProxyMiddleware } = require('http-proxy-middleware');

const app = express();
const PORT = process.env.PORT || 3000;

// Servir archivos estáticos
app.use('/static', express.static(path.join(__dirname, 'public', 'static')));

// Configuración dinámica de la API según el entorno
const API_URL = process.env.API_URL || 'http://157.230.10.37:5147';

// Proxy para la API
app.use('/api', createProxyMiddleware({
    target: API_URL,
    changeOrigin: true,
    pathRewrite: { '^/api': '/api' },
    logLevel: 'warn',
    onProxyReq: (proxyReq, req, res) => {
        console.log(`[PROXY] ${req.method} ${req.url} -> ${API_URL}${req.url}`);
    },
    onError: (err, req, res) => {
        console.error('[PROXY ERROR]', err.message);
        res.status(500).json({ 
            error: 'Error de conexión con el servidor API',
            detalle: err.message
        });
    }
}));

// Rutas para las páginas
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'index.html'));
});

app.get('/inicio', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'inicio.html'));
});

app.get('/privacidad', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'privacidad.html'));
});

app.get('/login', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'login.html'));
});

app.get('/micv', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'micv.html'));
});

app.get('/minegocio', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'minegocio.html'));
});

app.get('/miperfil', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'miperfil.html'));
});

app.get('/ayuda', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'ayuda.html'));
});

app.get('/registro', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'registro.html'));
});

app.get('/registro_empresa', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'registro_empresa.html'));
});

app.get('/negocio/:id', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'detalle_negocio.html'));
});

app.get('/notificaciones', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'notificaciones.html'));
});

app.get('/configuracion', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'pages', 'configuracion.html'));
});

// Iniciar servidor
app.listen(PORT, () => {
    console.log(`\n========================================`);
    console.log(`Servidor HOP corriendo en http://localhost:${PORT}`);
    console.log(`API Proxy -> ${API_URL}/api`);
    console.log(`========================================\n`);
});