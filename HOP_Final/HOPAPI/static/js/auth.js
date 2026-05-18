// Servicio de autenticación - ADAPTADO PARA TU API
const API_BASE = '/api';

class AuthService {
    static async login(email, password) {
        try {
            // Tu backend espera estos parámetros en el body como form-data
            const params = new URLSearchParams();
            params.append('email', email);
            params.append('password', password);
            
            const response = await fetch(`${API_BASE}/usuarios/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: params
            });
            
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || 'Credenciales incorrectas');
            }
            
            const usuario = await response.json();
            localStorage.setItem('hop_session', 'active');
            localStorage.setItem('hop_usuario', JSON.stringify(usuario));
            return usuario;
        } catch (error) {
            console.error('Error en login:', error);
            throw error;
        }
    }
    
    static async register(nombre, rolId, email, password) {
        try {
            const params = new URLSearchParams();
            params.append('nombre', nombre);
            params.append('rolId', rolId);
            params.append('email', email);
            params.append('password', password);
            
            const response = await fetch(`${API_BASE}/usuarios/registrar`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: params
            });
            
            if (!response.ok) {
                throw new Error('Error en el registro');
            }
            
            return await response.json();
        } catch (error) {
            console.error('Error en registro:', error);
            throw error;
        }
    }
    
    static logout() {
        localStorage.removeItem('hop_session');
        localStorage.removeItem('hop_usuario');
        window.location.href = '/';
    }
    
    static isAuthenticated() {
        return localStorage.getItem('hop_session') === 'active';
    }
    
    static getCurrentUser() {
        const user = localStorage.getItem('hop_usuario');
        return user ? JSON.parse(user) : null;
    }
}