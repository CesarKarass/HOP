const express = require('express');
const path = require('path');
const { createProxyMiddleware } = require('http-proxy-middleware');

const app = express();
const PORT = 3000;

// Servir archivos estáticos
app.use('/static', express.static(path.join(__dirname, 'public', 'static')));

// Proxy para la API de .NET (redirige /api/* a http://localhost:5147/api/*)
app.use('/api', createProxyMiddleware({
    target: 'http://localhost:5147',
    changeOrigin: true,
    // No reescribir la ruta, mantener /api
    pathRewrite: {
        '^/api': '/api'
    },
    // Logging para depuración
    logLevel: 'debug',
    onProxyReq: (proxyReq, req, res) => {
        console.log(`[PROXY] ${req.method} ${req.url} -> http://localhost:5147${req.url}`);
    },
    onError: (err, req, res) => {
        console.error('[PROXY ERROR]', err.message);
        res.status(500).json({ 
            error: 'No se pudo conectar con el servidor API', 
            detalle: err.message,
            sugerencia: 'Verifica que el backend de C# esté corriendo en http://localhost:5147'
        });
    }
}));

// Ruta de prueba para verificar que el proxy funciona
app.get('/api/test', (req, res) => {
    res.json({ message: 'Proxy funcionando correctamente' });
});

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
    console.log(`Proxy API -> http://localhost:5147/api`);
    console.log(`========================================\n`);
});