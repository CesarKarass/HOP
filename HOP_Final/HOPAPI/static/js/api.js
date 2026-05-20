// API Client para conectar con HOPAPI
const API_BASE_URL = 'http://localhost:5147/api';

class HOPAPI {
    constructor() {
        this.token = localStorage.getItem('hop_token');
    }

    async request(endpoint, options = {}) {
        const url = `${API_BASE_URL}${endpoint}`;
        const headers = {
            'Content-Type': 'application/json',
            ...options.headers
        };
        
        if (this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }
        
        const response = await fetch(url, {
            ...options,
            headers
        });
        
        if (!response.ok) {
            const error = await response.text();
            throw new Error(error || `Error ${response.status}`);
        }
        
        return response.json();
    }
    
    // ============ USUARIOS ============
    async registrar(nombre, rolId, email, password) {
        const params = new URLSearchParams({ nombre, rolId, email, password });
        return this.request(`/Usuarios/registrar?${params}`, { method: 'POST' });
    }
    
    async login(email, password) {
        const params = new URLSearchParams({ email, password });
        const result = await this.request(`/Usuarios/login?${params}`, { method: 'POST' });
        if (result && result.id) {
            this.token = `temp-${result.id}`;
            localStorage.setItem('hop_token', this.token);
            localStorage.setItem('hop_usuario', JSON.stringify(result));
            localStorage.setItem('hop_session', 'active');
        }
        return result;
    }
    
    async actualizarPerfil(usuarioId, nombreCompleto, bio, foto, tel) {
        const params = new URLSearchParams({ usuarioId, nombreCompleto, bio, foto, tel });
        return this.request(`/Usuarios/perfil?${params}`, { method: 'PUT' });
    }
    
    // ============ SERVICIOS ============
    async crearServicio(titulo, usuarioId, ubicacion, categoriaId, descripcion) {
        const params = new URLSearchParams({ titulo, usuarioId, ubicacion, categoriaId, descripcion });
        return this.request(`/Servicios?${params}`, { method: 'POST' });
    }
    
    async buscarServicios(filtros) {
        return this.request('/Servicios/buscar', {
            method: 'POST',
            body: JSON.stringify(filtros)
        });
    }
    
    async postularse(servicioId, prestadorId) {
        const params = new URLSearchParams({ servicioId, prestadorId });
        return this.request(`/Servicios/postular?${params}`, { method: 'POST' });
    }
    
    // ============ POSTULACIONES ============
    async aplicarPostulacion(servicioId, prestadorId) {
        const params = new URLSearchParams({ servicioId, prestadorId });
        return this.request(`/Postulaciones/aplicar?${params}`, { method: 'POST' });
    }
    
    async obtenerPostulacionesPorServicio(servicioId) {
        return this.request(`/Postulaciones/servicio/${servicioId}`);
    }
    
    // ============ CHAT ============
    async enviarMensaje(emisorId, receptorId, mensaje) {
        const params = new URLSearchParams({ emisorId, receptorId, mensaje });
        return this.request(`/Chat/enviar?${params}`, { method: 'POST' });
    }
    
    async obtenerNotificaciones(usuarioId) {
        return this.request(`/Chat/notificaciones/${usuarioId}`);
    }
    
    // ============ CATEGORÍAS ============
    async obtenerCategorias() {
        return this.request('/Categorias');
    }
    
    logout() {
        localStorage.removeItem('hop_token');
        localStorage.removeItem('hop_usuario');
        localStorage.removeItem('hop_session');
        this.token = null;
    }
    
    isAuthenticated() {
        return !!this.token && localStorage.getItem('hop_session') === 'active';
    }
    
    getCurrentUser() {
        return JSON.parse(localStorage.getItem('hop_usuario') || 'null');
    }
}

// Instancia global
const hopApi = new HOPAPI();