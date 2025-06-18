export const waterVertexShader = `
    precision highp float;

    // Attributes
    attribute vec3 position;
    attribute vec2 uv;
    attribute vec3 normal;

    // Uniforms
    uniform mat4 world;
    uniform mat4 worldView;
    uniform mat4 worldViewProjection;
    uniform float time;
    uniform float waveHeight;
    uniform float waveSpeed;

    // Varying
    varying vec2 vUV;
    varying vec3 vNormal;
    varying vec3 vPosition;

    void main(void) {
        vUV = uv;
        vNormal = normal;
        
        // 计算水波效果
        float wave = sin(position.x * 2.0 + time * waveSpeed) * 
                    cos(position.z * 2.0 + time * waveSpeed) * waveHeight;
        
        vec3 newPosition = position;
        newPosition.y += wave;
        
        vPosition = (world * vec4(newPosition, 1.0)).xyz;
        gl_Position = worldViewProjection * vec4(newPosition, 1.0);
    }
`;

export const waterFragmentShader = `
    precision highp float;

    varying vec2 vUV;
    varying vec3 vNormal;
    varying vec3 vPosition;

    uniform sampler2D diffuseSampler;
    uniform float time;
    uniform vec3 waterColor;
    uniform float alpha;

    void main(void) {
        // 基础颜色
        vec4 color = texture2D(diffuseSampler, vUV);
        
        // 添加法线效果
        vec3 normal = normalize(vNormal);
        float diffuse = max(0.0, dot(normal, vec3(0.0, 1.0, 0.0)));
        
        // 添加边缘光效果
        float rim = 1.0 - max(0.0, dot(normal, vec3(0.0, 0.0, 1.0)));
        rim = pow(rim, 3.0);
        
        // 合并效果
        color.rgb *= (0.5 + 0.5 * diffuse);
        color.rgb += rim * 0.2;
        
        // 应用水的颜色
        color.rgb *= waterColor;
        
        // 设置透明度
        color.a = alpha;
        
        gl_FragColor = color;
    }
`; 