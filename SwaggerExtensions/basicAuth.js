// Custom authentication UI for HDB API
(function() {
    'use strict';

    console.log('HDB API Swagger extensions loading...');

    // Wait for Swagger UI to load
    function initBasicAuth() {
        if (typeof window.ui === 'undefined') {
            setTimeout(initBasicAuth, 100);
            return;
        }

        // Create custom auth UI
        const authContainer = document.createElement('div');
        authContainer.id = 'hdb-auth-container';
        authContainer.innerHTML = `
            <div style="background: #f8f8f8; padding: 10px; margin-bottom: 10px; border: 1px solid #ddd; border-radius: 4px;">
                <h4 style="margin: 0 0 10px 0; color: #333;">HDB Authentication</h4>
                <div style="display: flex; gap: 10px; align-items: center;">
                    <div>
                        <label for="hdb-input" style="display: block; font-size: 12px; margin-bottom: 2px;">HDB:</label>
                        <input type="text" id="hdb-input" placeholder="FREEPDB1" 
                               style="padding: 4px; border: 1px solid #ccc; border-radius: 3px; width: 180px;" />
                    </div>
                    <div>
                        <label for="user-input" style="display: block; font-size: 12px; margin-bottom: 2px;">Username:</label>
                        <input type="text" id="user-input" placeholder="app_user" 
                               style="padding: 4px; border: 1px solid #ccc; border-radius: 3px; width: 120px;" />
                    </div>
                    <div>
                        <label for="pass-input" style="display: block; font-size: 12px; margin-bottom: 2px;">Password:</label>
                        <input type="password" id="pass-input" placeholder="<password>" 
                               style="padding: 4px; border: 1px solid #ccc; border-radius: 3px; width: 120px;" />
                    </div>
                    <button id="auth-button" style="padding: 4px 12px; background: #007acc; color: white; border: none; border-radius: 3px; cursor: pointer;">Set Auth</button>
                </div>
                <div id="auth-status" style="margin-top: 5px; font-size: 12px; color: #666;"></div>
            </div>
        `;

        // Insert before the main content
        const swaggerContainer = document.querySelector('.swagger-ui .topbar');
        if (swaggerContainer) {
            swaggerContainer.parentNode.insertBefore(authContainer, swaggerContainer.nextSibling);
        }

        // Add event listener
        document.getElementById('auth-button').addEventListener('click', function() {
            const hdb = document.getElementById('hdb-input').value;
            const user = document.getElementById('user-input').value;
            const pass = document.getElementById('pass-input').value;

            if (hdb && user && pass) {
                // Store auth info for request interceptor
                window.hdbAuth = { hdb, user, pass };
                document.getElementById('auth-status').textContent = `Auth set for HDB: ${hdb}, User: ${user}`;
                document.getElementById('auth-status').style.color = 'green';
            } else {
                document.getElementById('auth-status').textContent = 'Please fill all fields';
                document.getElementById('auth-status').style.color = 'red';
            }
        });

        // Modify ID inputs to textarea for better UX
        function modifyIdInputs() {
            console.log('modifyIdInputs called');

            // Clear default "string" values from all remaining inputs
            const allInputs = document.querySelectorAll('input[type="text"], input[type="number"]');
            allInputs.forEach(input => {
                if (input.value === 'string') {
                    input.value = '';
                    console.log(`Cleared default "string" value from input: ${input.name}`);
                }
            });
        }

        // Modify inputs immediately and also watch for dynamic content
        modifyIdInputs();
        
        // Watch for dynamically added inputs
        const observer = new MutationObserver((mutations) => {
            let shouldCheck = false;
            mutations.forEach(mutation => {
                if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                    // Check if any added nodes contain input elements
                    mutation.addedNodes.forEach(node => {
                        if (node.nodeType === Node.ELEMENT_NODE) {
                            if (node.tagName === 'INPUT' || node.querySelector('input')) {
                                shouldCheck = true;
                            }
                        }
                    });
                }
            });
            if (shouldCheck) {
                setTimeout(modifyIdInputs, 100);
            }
        });
        
        observer.observe(document.body, { childList: true, subtree: true });

        // Initialize when DOM is ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initBasicAuth);
        } else {
            initBasicAuth();
        }
    }
})();
